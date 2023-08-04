namespace PuzonnsThings.Models.Casino;

public class WheelOnRollModel
{
    public required WheelPointType WinnerPoint { get; set; }
    public required double CoinsWon { get; set; }
    public required long NextRollTimestamp { get; set; }
}

public enum WheelPointType
{
    Grey,
    Red,
    Blue,
    Yellow
}