namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeeOptionsMaxPlayerChange
{
    public required int MaxPlayersState { get; set; }
    public required YahtzeePlayerModel[] Players { get; set; } = new YahtzeePlayerModel[0];
}