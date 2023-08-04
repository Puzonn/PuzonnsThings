namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeePlayerModel
{
    public required string PlayerName { get; set; }
    public int Points { get; set; } = 0;
    public int GameTime { get; set; }  
    public required int UserId { get; set; }
    public int LobbyPlaceId { get; set; } = -1;
    public bool HasRound { get; set; } = false;
    public YahtzeeSettedPoint[] SettedPoints { get; set; } = new YahtzeeSettedPoint[0];
}