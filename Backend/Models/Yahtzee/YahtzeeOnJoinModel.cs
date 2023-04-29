namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public class YahtzeeOnJoinModel
{
    public YahtzeeDice[] Dices { get; set; } = new YahtzeeDice[0];
    public List<YahtzeeSettedPoint> SettedPoints { get; set; } = new List<YahtzeeSettedPoint>();
    public YahtzeePlayerModel[] Players { get; set; } = new YahtzeePlayerModel[0];

    public int Points { get; set; } = 0;
    public int RollCount { get; set; } = 2;
    public bool IsCreator { get; set; } = false;
    public bool GameStarted { get; set; } = false;
    public bool HasRound { get; set; } = false;
}