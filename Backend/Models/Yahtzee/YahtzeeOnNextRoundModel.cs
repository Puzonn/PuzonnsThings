namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeeOnNextRoundModel
{
    public bool HasRound { get; set; } = false;
    public YahtzeePlayerModel[] Players { get; set; } = new YahtzeePlayerModel[0];
    public YahtzeeDice[] Dices { get; set; } = new YahtzeeDice[0];
}