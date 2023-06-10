namespace Backend.Models.Casino;

public class WheelBetCallbackModel
{
    public required bool Success { get; set; }
    public required float Amount { get; set; }

    public static readonly WheelBetCallbackModel Unsuccessful =
        new WheelBetCallbackModel() { Amount = 0, Success = false}; 
}