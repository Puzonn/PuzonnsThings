namespace Backend.Models.Casino;

public class WheelClientModel
{
    public string ConnectionId { get; set; }
    public string Username { get; set; }

    public int UserId { get; }
    public bool HasAnyBet { get; set; }

    public Dictionary<WheelPointType, float> BettedPoints { get; private set; } = new(DefaultBets);

    private static readonly Dictionary<WheelPointType, float> DefaultBets = new Dictionary<WheelPointType, float>()
    {
        { WheelPointType.Blue, 0 },
        { WheelPointType.Red, 0 },
        { WheelPointType.Grey, 0 },
        { WheelPointType.Yellow, 0 }
    };

    public bool HasBetted(WheelPointType point)
    {
        return BettedPoints[point] > 0;
    }

    public void Bet(WheelPointType point, float amount)
    {
        BettedPoints[point] = amount;
        HasAnyBet = true;
    }

    public WheelClientModel(string connectionId, string username, int userId)
    {
        ConnectionId = connectionId;
        UserId = userId;
        Username = username;
    }

    public double CalculateWin(WheelPointType winPoint)
    {
        float sum = 0;

        foreach(var bet in BettedPoints.Keys)
        {
            if(bet == winPoint)
            {
                sum += BettedPoints[bet];
            }
            else
            {
                sum -= BettedPoints[bet];
            }
        }

        return sum;
    }

    public void ClearBets()
    {
        BettedPoints = new(DefaultBets);
        HasAnyBet = false;
    }
}
