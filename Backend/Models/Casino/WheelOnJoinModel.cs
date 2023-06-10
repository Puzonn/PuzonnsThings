namespace Backend.Models.Casino;

public class WheelOnJoinModel
{
    /// <summary>
    /// Timestamp of roll
    /// </summary>
    public required long RollTimestamp { get; set; }
    /// <summary>
    /// Client Bets
    /// </summary>
    public required IEnumerable<WheelBetOnPlayerBetModel> UserBets { get; set; }
    /// <summary>
    /// All bets of all clients
    /// </summary>
    public required IEnumerable<WheelBetOnPlayerBetModel> AllBets { get; set; }
}