using Backend.Models;
using Backend.Services;

namespace PuzonnsThings.Models.Yahtzee;

public class YahtzeeLobby : ILobbyCollectable
{
    public readonly List<YahtzeePlayer> Players = new List<YahtzeePlayer>(4);

    public YahtzeePlayer? PlayerRound { get; set; }

    public int ActivePlayers { get => Players.Count; }

    public uint LobbyId { get; }
    public readonly uint CreatorId;

    public bool GameEnded { get; set; } = false;
    public bool GameStarted { get; set; } = false;
    public bool IsInitialized { get; private set; }

    public DateTime LastLobbySnapshot { get; set; } = DateTime.Now;

    public void StartGame()
    {
        GameStarted = true;

        PlayerRound = Players[Random.Shared.Next(0, Players.Count-1)];

        foreach (YahtzeePlayer player in Players)
        {
            foreach (YahtzeeDice dice in player.Dices)
            {
                dice.Roll();
            }
        }
    }

    public YahtzeePlayer GetLastPlayer() => Players.Last();

    public YahtzeePlayerModel[] GetPlayersModels()
    {
        YahtzeePlayerModel[] players = new YahtzeePlayerModel[Players.Count];

        for (int i = 0; i < Players.Count; i++)
        {
            YahtzeePlayer lobbyPlayer = Players[i];

            players[i] = new YahtzeePlayerModel()
            {
                HasRound = PlayerRound == lobbyPlayer,
                PlayerName = lobbyPlayer.PlayerName,
                Points = lobbyPlayer.Points,
                SettedPoints = lobbyPlayer.SettedPoints.ToArray()
            };
        }

        return players;
    }

    public void NextRound()
    {
        if (!GameStarted || PlayerRound is null)
        {
            return;
        }

        int playerIndex = Players.IndexOf(PlayerRound);

        if (playerIndex + 1 == Players.Count)
        {
            PlayerRound = Players[0];
        }
        else
        {
            PlayerRound = Players[playerIndex + 1];
        }

        foreach(YahtzeePlayer player in Players)
        {
            foreach(YahtzeeDice dice in player.Dices)
            {
                dice.Roll();
            }
        }
    }

    public void Initialize(YahtzeePlayer creator)
    {
        Players.Add(creator);
        IsInitialized = true;
    }

    public YahtzeeLobby(uint creatorId, uint lobbyId)
    {
        CreatorId = creatorId;
        LobbyId = lobbyId;
    }
}