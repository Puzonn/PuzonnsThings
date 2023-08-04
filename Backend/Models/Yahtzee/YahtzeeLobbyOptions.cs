using System.Text.Json.Serialization;

namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeeLobbyOptions
{
    /// <summary>
    /// Max players in game
    /// </summary>
    public required int MaxPlayersCount { get; set; }
    /// <summary>
    /// GameTime in seconds
    /// </summary>
    public required int GameTime { get; set; }

    /// <summary>
    /// Default options
    /// </summary>
    [JsonIgnore]
    public readonly static YahtzeeLobbyOptions Default = new YahtzeeLobbyOptions() { MaxPlayersCount = 2, GameTime = 15 };
}