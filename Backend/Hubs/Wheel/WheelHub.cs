using PuzonnsThings.Hubs.Extensions;
using PuzonnsThings.Models.Casino;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PuzonnsThings.Models;
using PuzonnsThings.Services;
using PuzonnsThings.Repositories;

namespace PuzonnsThings.Hubs.Wheel;

[Authorize]
public class WheelHub : Hub
{
    private readonly WheelService wheelService;
    private readonly UserRepository userRepository;

    public WheelHub(WheelService service, UserRepository repository)
    {
        wheelService = service;
        userRepository = repository;
    }

    public async Task<WheelBetCallbackModel> Bet(WheelBetModel bet)
    {
        User? user = await Context.GetUser(userRepository);

        if (user is null)
        {
            Context.Abort();
            return WheelBetCallbackModel.Unsuccessful;
        }

        if (user.Balance >= bet.Amount)
        {
            if (!wheelService.AddBet(user.Id, bet.WheelPoint, bet.Amount))
            {
                return WheelBetCallbackModel.Unsuccessful;
            }

            user.Balance -= bet.Amount;
            userRepository.UpdateUserAsync(user);

            WheelBetCallbackModel callback = new WheelBetCallbackModel()
            {
                Amount = bet.Amount,
                Success = true
            };

            await OnPlayerBet(user.Username, bet.Amount, bet.WheelPoint);

            return callback;
        }

        return WheelBetCallbackModel.Unsuccessful;
    }

    public override async Task OnConnectedAsync()
    {
        User? user = await Context.GetUser(userRepository);

        if (user is null)
        {
            Context.Abort();
            return;
        }

        wheelService.AddOrUpdatePlayer(user.Username, Context.ConnectionId, user.Id);

        List<WheelBetOnPlayerBetModel> bets = new List<WheelBetOnPlayerBetModel>(wheelService.Clients.Count);

        foreach (var client in wheelService.Clients.Values)
        {
            if (!client.HasAnyBet || client.UserId == user.Id)
            {
                continue;
            }

            foreach (var bet in client.BettedPoints)
            {
                if (bet.Value > 0)
                {
                    bets.Add(new WheelBetOnPlayerBetModel()
                    {
                        Amount = bet.Value,
                        Username = client.Username,
                        WheelPoint = bet.Key
                    });
                }
            }
        }

        List<WheelBetOnPlayerBetModel> userBets = new List<WheelBetOnPlayerBetModel>(4);

        foreach (var bet in wheelService.Clients.Values.Where(x => x.UserId == user.Id).First().BettedPoints)
        {
            if (bet.Value > 0)
            {
                userBets.Add(new WheelBetOnPlayerBetModel()
                {
                    Username = user.Username,
                    Amount = bet.Value,
                    WheelPoint = bet.Key
                });
            }
        }

        WheelOnJoinModel joinModel = new WheelOnJoinModel()
        {
            RollTimestamp = wheelService.GetRollTimestamp(),
            AllBets = bets,
            UserBets = userBets
        };

        await Clients.Caller.SendAsync("OnJoin", joinModel);

        await base.OnConnectedAsync();
    }

    private async Task OnPlayerBet(string username, float amount, WheelPointType point)
    {
        WheelBetOnPlayerBetModel bet = new WheelBetOnPlayerBetModel()
        {
            Username = username,
            Amount = amount,
            WheelPoint = point
        };
        await Clients.All.SendAsync("OnPlayerBet", bet);
    }
}