namespace PuzonnsThings.Models.Lobbies;

public readonly record struct LobbyType(string Name, string ApiName, int MaxPlayerCount);

public static class LobbyTypes
{
    public static LobbyType Yahtzee = new LobbyType("Yahtzee", "yahtzee", 4);
    public static LobbyType WatchTogether = new LobbyType("Watch Together", "watchtogether", -1);
}