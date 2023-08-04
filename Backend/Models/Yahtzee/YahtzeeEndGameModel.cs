namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeeEndGameModel
{
    public required string WinnerUsername { get; set; }
    public required Dictionary<string, float> Leaderboard;
}