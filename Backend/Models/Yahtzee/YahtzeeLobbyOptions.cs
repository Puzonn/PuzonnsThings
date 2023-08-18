using System.Text.Json.Serialization;
using Backend.Models.Interfaces;

namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeeLobbyOptions : ILobbyOptions
{
    /// <summary>
    ///     Default options
    /// </summary>
    [JsonIgnore] public static readonly YahtzeeLobbyOptions Default = new(2, 4, 60, 240)
    {
        GameTime = 60,
        MaxPlayers = 2
    };

    public YahtzeeLobbyOptions(uint minPlayersLimit, uint maxPlayersLimit, uint minGameTimeLimit, uint maxGameTimeLimit)
    {
        MaxPlayersLimit = maxPlayersLimit;
        MinPlayersLimit = minPlayersLimit;

        MaxGameTimeLimit = maxGameTimeLimit;
        MinGameTimeLimit = minGameTimeLimit;
    }

    /// <summary>
    ///     GameTime in seconds
    /// </summary>
    public int GameTime { get; set; }

    [JsonIgnore] public uint MaxGameTimeLimit { get; }

    [JsonIgnore] public uint MinGameTimeLimit { get; }

    /// <summary>
    ///     Max players in game
    /// </summary>
    [JsonIgnore]
    public uint MaxPlayersLimit { get; }

    /// <summary>
    ///     Min players in game
    /// </summary>
    [JsonIgnore]
    public uint MinPlayersLimit { get; }

    public uint MaxPlayers { get; set; }

    public bool IsPublic { get; set; }
}