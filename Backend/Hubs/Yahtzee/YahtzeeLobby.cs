using PuzonnsThings.Models.Yahtzee;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;
using Backend.Models;

namespace PuzonnsThings.Hubs.Yahtzee;

public class YahtzeeLobby : Lobby<YahtzeePlayer, YahtzeeLobbyOptions>
{
    private Timer RoundTimer { get; set; } = new Timer();

    private readonly IHubContext<YahtzeeHub> HubContext;

    public YahtzeePlayer? PlayerRound { get; set; }

    public bool GameEnded { get; set; } = false;
    public bool GameStarted { get; private set; } = false;
    public bool IsInitialized { get; private set; }

    private int _startedGameTime;

    public int GameTime
    {
        get
        {
            return GameStarted ? _startedGameTime : LobbyOptions.GameTime;
        }
        set
        {
            if (!GameStarted)
            {
                LobbyOptions.GameTime = value;
            }
            else
            {
                _startedGameTime = value;
            }
        }
    }

    public YahtzeeLobby(IHubContext<YahtzeeHub> hubContext, uint creatorId, uint lobbyId) :
        base(creatorId, lobbyId, YahtzeeLobbyOptions.Default)
    {
        HubContext = hubContext;
    }

    public bool IsReadyToStart() => LobbyPlaces.Count >= LobbyOptions.MinPlayersLimit && LobbyPlaces.All(x => x.Value != -1);


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
            leaderboard.Add(lobbyPlayer.Username, (float)(lobbyPlayer.Points * 0.1 * Players.Count));
        }

        return new YahtzeeEndGameModel()
        {
            Leaderboard = leaderboard,
            WinnerUsername = winner.Username
        };
    }

    public async Task EndGame()
    {
        YahtzeeEndGameModel endgame = CreateEndgameModel();

        foreach (YahtzeePlayer lobbyPlayer in Players)
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

        _startedGameTime = LobbyOptions.GameTime;

        InitTimer();

        PlayerRound = Players[Random.Shared.Next(0, Players.Count - 1)];

        Players.ForEach(x => x.Dices.ToList().ForEach(x => x.Roll()));

        return true;
    }

    private void InitTimer()
    {
        Players.ForEach(x => x.GameTime = LobbyOptions.GameTime);

        RoundTimer = new Timer(1000);
        RoundTimer.AutoReset = true;

        RoundTimer.Elapsed += OnTimerEnd;

        RoundTimer.Start();
    }

    private async void OnTimerEnd(object? sender, ElapsedEventArgs e)
    {
        if (PlayerRound is not null)
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
        Debug.Write(Players);
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
                Avatar = lobbyPlayer.Avatar,
                HasRound = PlayerRound == lobbyPlayer,
                GameTime = lobbyPlayer.GameTime,
                Username = lobbyPlayer.Username,
                Points = lobbyPlayer.Points,
                PlacedPoints = lobbyPlayer.PlacedPoints.ToArray(),
                LobbyPlaceId = lobbyPlaceId,
                UserId = lobbyPlayer.UserId,
            };
        }

        return players;
    }

    private YahtzeePlayer GetNextPlayer(YahtzeePlayer? player)
    {
        if (player is null)
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

        if (!GameStarted || PlayerRound is null)
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