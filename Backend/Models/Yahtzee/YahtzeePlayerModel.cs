namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeePlayerModel
{
    public required string Username { get; set; }
    public required string Avatar { get; set; }
    public required int UserId { get; set; }
    public int Points { get; set; } = 0;
    public int GameTime { get; set; }  
    public int LobbyPlaceId { get; set; } = -1;
    public bool HasRound { get; set; } = false;
    public YahtzeePlacedPoint[] PlacedPoints { get; set; } = new YahtzeePlacedPoint[0];
}