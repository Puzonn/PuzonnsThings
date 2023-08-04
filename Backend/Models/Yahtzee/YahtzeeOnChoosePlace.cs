namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeeOnChoosePlace
{
    public required YahtzeePlayerModel[] Players { get; set; }
    public required bool StartState { get; set; } = false;
}