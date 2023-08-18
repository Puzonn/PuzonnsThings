using System.Timers;
using Backend.Models;
using Microsoft.AspNetCore.SignalR;
using PuzonnsThings.Models.Yahtzee;
using Timer = System.Timers.Timer;

namespace PuzonnsThings.Hubs.Yahtzee;

public class YahtzeeLobby : Lobby<YahtzeePlayer, YahtzeeLobbyOptions>
{
    private readonly IHubContext<YahtzeeHub> HubContext;

    private int _startedGameTime;

    public YahtzeeLobby(IHubContext<YahtzeeHub> hubContext, uint creatorId, uint lobbyId) :
        base(creatorId, lobbyId, YahtzeeLobbyOptions.Default)
    {
        HubContext = hubContext;
    }

    private Timer RoundTimer { get; set; } = new();

    public YahtzeePlayer? PlayerRound { get; set; }

    public bool GameEnded { get; set; }
    public bool GameStarted { get; private set; }
    public bool IsInitialized { get; private set; }

    public int GameTime
    {
        get => GameStarted ? _startedGameTime : LobbyOptions.GameTime;
        set => LobbyOptions.GameTime = value;
    }

    public bool IsReadyToStart()
    {
        return LobbyPlaces.Count >= LobbyOptions.MinPlayersLimit && LobbyPlaces.All(x => x.Value != -1);
    }


    /// <summary>
    ///     Creates an instance of the YahtzeeEndGameModel that represents the endgame state and leaderboard.
    /// </summary>
    /// <returns>The YahtzeeEndGameModel instance.</returns>
    private YahtzeeEndGameModel CreateEndgameModel()
    {
        var winner = Players.OrderBy(x => x.Points).First(x => x.CanPlay);
        var leaderboard = new Dictionary<string, float>();

        foreach (var lobbyPlayer in Players)
            leaderboard.Add(lobbyPlayer.Username, (float)(lobbyPlayer.Points * 0.1 * Players.Count));

        return new YahtzeeEndGameModel
        {
            Leaderboard = leaderboard,
            WinnerUsername = winner.Username
        };
    }

    public bool ChangeGameTime(int state)
    {
        if (state <= LobbyOptions.MaxGameTimeLimit && state >= LobbyOptions.MinPlayersLimit && !GameStarted)
        {
            LobbyOptions.GameTime = state;
            return true;
        }

        return false;
    }

    public async Task EndGame()
    {
        var endgame = CreateEndgameModel();

        foreach (var lobbyPlayer in Players)
            await HubContext.Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnEndGame", endgame);
    }

    public bool StartGame()
    {
        if (!IsReadyToStart() && !GameStarted) return false;

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
            if (PlayerRound.GameTime == 0)
            {
                PlayerRound.CanPlay = false;

                await NextRound();
            }

            PlayerRound.GameTime -= 1;
        }
    }

    public YahtzeePlayer GetLastPlayer()
    {
        return Players.Last();
    }

    public YahtzeePlayerModel[] GetPlayersModels()
    {
        var players = new YahtzeePlayerModel[Players.Count];

        for (var i = 0; i < Players.Count; i++)
        {
            var lobbyPlayer = Players[i];
            var lobbyPlaceId = -1;

            if (LobbyPlaces.ContainsKey(lobbyPlayer.UserId))
                lobbyPlaceId = LobbyPlaces[lobbyPlayer.UserId];

            players[i] = new YahtzeePlayerModel
            {
                Avatar = lobbyPlayer.Avatar,
                HasRound = PlayerRound == lobbyPlayer,
                GameTime = lobbyPlayer.GameTime,
                Username = lobbyPlayer.Username,
                Points = lobbyPlayer.Points,
                PlacedPoints = lobbyPlayer.PlacedPoints.ToArray(),
                LobbyPlaceId = lobbyPlaceId,
                UserId = lobbyPlayer.UserId
            };
        }

        return players;
    }

    private YahtzeePlayer GetNextPlayer(YahtzeePlayer? player)
    {
        if (player is null) throw new InvalidOperationException("PlayerRound is null, which is an invalid state.");

        var playerIndex = Players.IndexOf(player);

        if (playerIndex + 1 == Players.Count)
            return Players[0];
        return Players[playerIndex + 1];
    }

    private bool CheckWin()
    {
        return Players.Count(x => !x.CanPlay) == Players.Count - 1;
    }

    public async Task NextRound()
    {
        if (CheckWin())
        {
            foreach (var lobbyPlayer in Players)
                await HubContext.Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnEndGame", CreateEndgameModel());

            GameEnded = true;
            return;
        }

        var nextPlayer = GetNextPlayer(PlayerRound);

        while (!nextPlayer.CanPlay) nextPlayer = GetNextPlayer(nextPlayer);

        if (!GameStarted || PlayerRound is null)
            throw new InvalidOperationException("Game is not started, which is an invalid state.");

        PlayerRound.RollCount = 2;

        PlayerRound = nextPlayer;

        foreach (var player in Players)
        foreach (var dice in player.Dices)
            dice.Roll();
    }

    public void Initialize(YahtzeePlayer creator)
    {
        Players.Add(creator);
        IsInitialized = true;
    }
}