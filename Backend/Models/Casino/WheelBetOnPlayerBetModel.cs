namespace PuzonnsThings.Models.Casino;

public class WheelBetOnPlayerBetModel
{
    public required string Username { get; set; }
    public required float Amount { get; set; }
    public required WheelPointType WheelPoint { get; set; }
}
