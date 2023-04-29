namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public class YahtzeePlayerModel
{
    public string PlayerName { get; set; } = string.Empty;
    public int Points { get; set; } = 0;
    public bool HasRound { get; set; } = false;
    public YahtzeeSettedPoint[] SettedPoints { get; set; } = new YahtzeeSettedPoint[0];
}