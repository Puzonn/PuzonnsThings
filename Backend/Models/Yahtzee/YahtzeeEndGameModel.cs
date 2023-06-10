namespace Backend.Models.Yahtzee;

[Serializable]
public class YahtzeeEndGameModel
{
    public required string WinnerUsername { get; set; }
    public required float CoinsGotten { get; set; }
}