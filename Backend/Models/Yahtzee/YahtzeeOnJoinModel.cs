namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeeOnJoinModel
{
    public YahtzeeDice[] Dices { get; set; } = new YahtzeeDice[0];
    public List<YahtzeePlacedPoint> PlacedPoints { get; set; } = new List<YahtzeePlacedPoint>();
    public YahtzeePlayerModel[] Players { get; set; } = new YahtzeePlayerModel[0];
    public YahtzeeLobbyOptions Options { get; set; } = YahtzeeLobbyOptions.Default;

    public int Points { get; set; } = 0;
    public int RollCount { get; set; } = 2;
    public bool IsCreator { get; set; } = false;
    public bool GameStarted { get; set; } = false;
    public bool HasRound { get; set; } = false;
    public bool StartState { get; set; } = false;
}