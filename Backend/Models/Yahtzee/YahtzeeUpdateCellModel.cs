namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeeUpdateCellModel
{
    public YahtzeePointType PointType { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Points { get; set; } = 0;
}
