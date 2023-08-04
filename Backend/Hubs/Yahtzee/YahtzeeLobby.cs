using PuzonnsThings.Models;
using PuzonnsThings.Models.Yahtzee;
using PuzonnsThings.Services;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace PuzonnsThings.Hubs.Yahtzee;

public class YahtzeeLobby : ILobbyCollectable, ILobbyPlace
{
    public readonly List<YahtzeePlayer> Players = new List<YahtzeePlayer>(MAX_PLAYERS);
    /// <summary>
    /// Dictionary of places where key is userId and value is placeId
    /// </summary>
    public Dictionary<int, int> LobbyPlaces { get; } = new Dictionary<int, int>(MAX_PLAYERS);
    public YahtzeeLobbyOptions Options { get; private set; } = YahtzeeLobbyOptions.Default;

    private const int MAX_PLAYERS = 5;
    private const int MIN_PLAYERS = 2;

    private Timer RoundTimer { get; set; } = new Timer();
    
    private readonly IHubContext<YahtzeeHub> HubContext;

    public YahtzeePlayer? PlayerRound { get; set; }
    public DateTime LastLobbySnapshot { get; set; } = DateTime.Now;

    public int ActivePlayers { get => Players.Count; }

    public uint LobbyId { get; }
    public readonly uint CreatorId;

    public bool GameEnded { get; set; } = false;
    public bool GameStarted { get; private set; } = false;
    public bool IsInitialized { get; private set; }

    public int MaxPlayers
    {
        get => Options.MaxPlayersCount;
        set => Options.MaxPlayersCount = value;
    }

    private int _startedGameTime;

    public int GameTime
    {
        get
        {
            return GameStarted ? _startedGameTime : Options.GameTime;
        }
        set
        {
            if (!GameStarted)
            {
                Options.GameTime = value;
            }
            else
            {
                _startedGameTime = value;
            }
        }
    }

    public YahtzeeLobby(IHubContext<YahtzeeHub> hubContext, uint creatorId, uint lobbyId)
    {
        HubContext = hubContext;

        CreatorId = creatorId;
        LobbyId = lobbyId;
    }

    /// <summary>
    /// Chooses a lobby place for a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="placeId">The ID of the lobby place to choose.</param>
    /// <returns>
    /// True if the lobby place was successfully chosen, false if the user already has a lobby place.
    /// </returns
    public bool ChoosePlace(int userId, int placeId)
    {
        if (LobbyPlaces.ContainsKey(userId) || IsLobbyPlaceOccupied(placeId))
        {
            return false;
        }

        if(placeId >= Options.MaxPlayersCount)
        {
            return false;
        }

        LobbyPlaces.Add(userId, placeId);

        return true;
    }

    public bool IsReadyToStart() => LobbyPlaces.Count >= MIN_PLAYERS && LobbyPlaces.All(x => x.Value != -1);

    /// <summary>
    /// Retrieves the lobby place of a player.
    /// </summary>
    /// <param name="userId">The ID of the player.</param>
    /// <returns>
    /// The lobby place occupied by the player, or -1 if the player is not in any lobby place.
    /// </returns>
    public int GetPlayerLobbyPlace(int userId)
    {
        return LobbyPlaces.ContainsKey(userId) ? LobbyPlaces[userId] : -1;
    }

    /// <summary>
    /// Removes a player from the lobby place.
    /// </summary>
    /// <param name="userId">The ID of the user to be removed from the lobby place.</param>
    public void RemovePlayerFromLobbyPlace(int userId)
    {
        if (GetPlayerLobbyPlace(userId) != -1)
        {
            LobbyPlaces.Remove(userId);
        }
    }

    /// <summary>
    /// Creates an instance of the YahtzeeEndGameModel that represents the endgame state and leaderboard.
    /// </summary>
    /// <returns>The YahtzeeEndGameModel instance.</returns>
    public YahtzeeEndGameModel CreateEndgameModel()
    {
        YahtzeePlayer winner = Players.OrderBy(x => x.Points).Where(x => x.CanPlay).First();
        Dictionary<string, float> leaderboard = new Dictionary<string, float>();

        foreach (YahtzeePlayer lobbyPlayer in Players)
        {
            leaderboard.Add(lobbyPlayer.PlayerName, (float)(lobbyPlayer.Points * 0.1 * Players.Count));
        }

        return new YahtzeeEndGameModel()
        {
            Leaderboard = leaderboard,
            WinnerUsername = winner.PlayerName
        };
    }

    /// <summary>
    /// Checks if a lobby place is currently occupied.
    /// </summary>
    /// <param name="placeId">The ID of the lobby place to check.</param>
    /// <returns>
    /// True if the lobby place is occupied, false otherwise.
    /// </returns
    public bool IsLobbyPlaceOccupied(int placeId)
    {
        return LobbyPlaces.ContainsValue(placeId);
    }

    public bool IsLobbyPlaceOccupiedByOtherPlayer(int userId, int placeId)
    {
        if (LobbyPlaces.ContainsValue(placeId))
        {
            return LobbyPlaces.FirstOrDefault(x => x.Value == placeId).Key == userId;
        }

        return false;
    }

    /// <summary>
    /// Changes count of maximum players in game. Dose not check if user is creator.
    /// </summary>
    /// <param name="count">Max players count</param>
    /// <returns>Returns true when count pass validating</returns>
    public bool ChangeMaxPlayers(int count)
    {
        bool validate = count <= MAX_PLAYERS && count >= MIN_PLAYERS;

        if (validate)
        {
            LobbyPlaces.Clear();

            Options.MaxPlayersCount = count;
        }

        return validate;
    }

    /// <summary>
    /// Ends a game 
    /// </summary>
    public async Task EndGame()
    {
        YahtzeeEndGameModel endgame = CreateEndgameModel();

        foreach(YahtzeePlayer lobbyPlayer in Players)
        {
            await HubContext.Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnEndGame", endgame);
        }
    }

    public bool StartGame()
    {
        if (!IsReadyToStart() && !GameStarted)
        {
            return false;
        }

        GameStarted = true;

        _startedGameTime = Options.GameTime;

        InitTimer();

        PlayerRound = Players[Random.Shared.Next(0, Players.Count - 1)];

        Players.ForEach(x => x.Dices.ToList().ForEach(x => x.Roll()));

        return true;
    }

    private void InitTimer()
    {
        Players.ForEach(x => x.GameTime = Options.GameTime);

        RoundTimer = new Timer(1000);
        RoundTimer.AutoReset = true;

        RoundTimer.Elapsed += OnTimerEnd;

        RoundTimer.Start();
    }

    private async void OnTimerEnd(object? sender, ElapsedEventArgs e)
    {
        if(PlayerRound is not null)
        {
            Debug.WriteLine($"Time is on, PlayerRound game time: {PlayerRound.GameTime}");

            if (PlayerRound.GameTime == 0)
            {
                PlayerRound.CanPlay = false;

                await NextRound();
            }

            PlayerRound.GameTime -= 1;
        }
    }

    public YahtzeePlayer GetLastPlayer() => Players.Last();

    public YahtzeePlayerModel[] GetPlayersModels()
    {
        YahtzeePlayerModel[] players = new YahtzeePlayerModel[Players.Count];

        for (int i = 0; i < Players.Count; i++)
        {
            YahtzeePlayer lobbyPlayer = Players[i];
            int lobbyPlaceId = -1;

            if (LobbyPlaces.ContainsKey(lobbyPlayer.UserId))
            {
                lobbyPlaceId = LobbyPlaces[lobbyPlayer.UserId];
            }

            players[i] = new YahtzeePlayerModel()
            {
                HasRound = PlayerRound == lobbyPlayer,
                GameTime = lobbyPlayer.GameTime,
                PlayerName = lobbyPlayer.PlayerName,
                Points = lobbyPlayer.Points,
                SettedPoints = lobbyPlayer.SettedPoints.ToArray(),
                LobbyPlaceId = lobbyPlaceId,
                UserId = lobbyPlayer.UserId,
            };
        }

        return players;
    }

    private YahtzeePlayer GetNextPlayer(YahtzeePlayer? player)
    {
        if(player is null)
        {
            throw new InvalidOperationException("PlayerRound is null, which is an invalid state.");
        }

        int playerIndex = Players.IndexOf(player);

        if (playerIndex + 1 == Players.Count)
        {
            return Players[0];
        }
        else
        {
            return Players[playerIndex + 1];
        }
    }

    public bool CheckWin()
    {
        return Players.Where(x => !x.CanPlay).Count() == Players.Count - 1;
    }

    public async Task NextRound()
    {
        if (CheckWin())
        {
            foreach (YahtzeePlayer lobbyPlayer in Players)
            {
                await HubContext.Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnEndGame", CreateEndgameModel());
            }

            GameEnded = true;
            return;
        }

        YahtzeePlayer nextPlayer = GetNextPlayer(PlayerRound);

        while (!nextPlayer.CanPlay)
        {
            nextPlayer = GetNextPlayer(nextPlayer);
        }

        if(!GameStarted || PlayerRound is null)
        {
            throw new InvalidOperationException("Game is not started, which is an invalid state.");
        }

        PlayerRound.RollCount = 2;

        PlayerRound = nextPlayer;

        foreach (YahtzeePlayer player in Players)
        {
            foreach (YahtzeeDice dice in player.Dices)
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
}