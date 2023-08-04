using PuzonnsThings.Hubs.Wheel;
using PuzonnsThings.Models.Casino;
using Microsoft.AspNetCore.SignalR;
using System.Timers;
using Timer = System.Timers.Timer;

namespace PuzonnsThings.Services;

public class WheelService
{
    public readonly Dictionary<int, WheelClientModel> Clients = new Dictionary<int, WheelClientModel>();

    public Timer SpinTimer { get; }
    public float SecondsToSpin { get; private set; } = 30;

    private const int TimeToBet = 30;
    private const int TimeToSpin = 8;

    public void AddOrUpdatePlayer(string username, string connectionId, int userId)
    {
        if (Clients.ContainsKey(userId))
        {
            Clients[userId].ConnectionId = connectionId;
        }
        else
        {
            Clients.Add(userId, new WheelClientModel(connectionId, username, userId));
        }
    }

    public bool AddBet(int userId, WheelPointType point, float amount)
    {
        if (Clients[userId].HasBetted(point))
        {
            return false;
        }

        Clients[userId].Bet(point, amount);

        return true;
    }

    public WheelService(IHubContext<WheelHub> wheelHub)
    {
        SpinTimer = new Timer(1000);
        SpinTimer.Elapsed += async (source, e) => await OnTimeElapsed(e, wheelHub);
        SpinTimer.Start();
    }

    public async Task OnTimeElapsed(ElapsedEventArgs e, IHubContext<WheelHub> hub)
    {
        SecondsToSpin -= 1;

        if (SecondsToSpin <= 0)
        {
            WheelPointType winPoint = WheelPointType.Grey;

            double blueProb = 0.2156;
            double redProb = 0.3765;
            double yellowProb = 0.04;

            double randomNumber = Random.Shared.NextDouble();

            if (randomNumber < blueProb)
            {
                winPoint = WheelPointType.Blue;
            }
            else if (randomNumber < blueProb + redProb)
            {
                winPoint = WheelPointType.Red;
            }
            else if (randomNumber < blueProb + redProb + yellowProb)
            {
                winPoint = WheelPointType.Yellow;
            }

            foreach (var client in Clients.Values)
            {
                WheelOnRollModel roll = new WheelOnRollModel()
                {
                    CoinsWon = client.CalculateWin(winPoint),
                    WinnerPoint = winPoint,
                    NextRollTimestamp = GetRollTimestamp(TimeToBet + TimeToSpin)
                };

                await hub.Clients.Client(client.ConnectionId).SendAsync("OnRoll", roll);

                OnRoll();
            }
            SecondsToSpin = TimeToBet + TimeToSpin;
        }
    }

    public void OnRoll()
    {
        foreach (var client in Clients.Values)
        {
            client.ClearBets();
        }
    }

    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public long GetRollTimestamp(int addSeconds = 0)
    {
        TimeSpan elapsedTime = DateTime.UtcNow.AddSeconds(SecondsToSpin + addSeconds) - Epoch;
        return (long)elapsedTime.TotalSeconds;
    }
}