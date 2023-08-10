namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeePlacedPoint
{
    public YahtzeePointType Point { get; set; }
    public int PointsFromPoint { get; set; }
}