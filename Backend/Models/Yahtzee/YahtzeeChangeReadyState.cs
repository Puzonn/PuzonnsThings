namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeeChangeReadyState
{
    public required string PlayerName { get; set; }
    public required bool State { get; set; }
}