using Backend.Models.Interfaces;
using System.Text.Json.Serialization;

namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeeLobbyOptions : ILobbyOptions
{
    [JsonIgnore]
    /// <summary>
    /// Max players in game
    /// </summary>
    public uint MaxPlayersLimit { get; }

    [JsonIgnore]
    /// <summary>
    /// Min players in game
    /// </summary>
    public uint MinPlayersLimit { get; }

    public uint MaxPlayers { get; set; }

    /// <summary>
    /// GameTime in seconds
    /// </summary>
    public int GameTime { get; set; }

    [JsonIgnore]
    public uint MaxGameTimeLimit { get; }

    [JsonIgnore]
    public uint MinGameTimeLimit { get; }

    public bool IsPublic { get; set; }

    /// <summary>
    /// Default options
    /// </summary>
    [JsonIgnore]
    public readonly static YahtzeeLobbyOptions Default = new YahtzeeLobbyOptions(2, 4, 60, 240)
    {
        GameTime = 60,
        MaxPlayers = 2,
    };

    public YahtzeeLobbyOptions(uint minPlayersLimit, uint maxPlayersLimit, uint maxGametimeLimit, uint minGameTimeLimit)
    {
        MaxPlayersLimit = maxPlayersLimit;
        MinPlayersLimit = minPlayersLimit;
        MaxGameTimeLimit = maxGametimeLimit;
        MinGameTimeLimit = minGameTimeLimit;
    }
}