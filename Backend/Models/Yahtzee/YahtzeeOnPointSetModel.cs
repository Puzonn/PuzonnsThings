namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public class YahtzeeOnPointSetModel
{
    public bool IsSuccessFul { get; set; }
    public int Points { get; set; }
    public int PointsFromPoint { get; set; }
    public YahtzeePointType Point { get; set; }

    public static readonly YahtzeeOnPointSetModel UnSuccessFul = new YahtzeeOnPointSetModel()
    {
        IsSuccessFul = false,
    };
}