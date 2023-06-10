using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PuzonnsThings.Models.Yahtzee;
using PuzonnsThings.Databases;
using PuzonnsThings.Models;
using TodoApp.Repositories;
using Backend.Models.Yahtzee;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Backend.Models.Lobbies;

namespace PuzonnsThings.Hubs;

[Authorize]
public class YahtzeeHub : Hub
{
    private static readonly Dictionary<int, YahtzeePlayer> users = new Dictionary<int, YahtzeePlayer>();
    private static readonly Dictionary<uint, YahtzeeLobby> rooms = new Dictionary<uint, YahtzeeLobby>();

    private readonly DatabaseContext _context;
    private readonly UserRepository _repository;

    public static void AddRoom(uint id, YahtzeeLobby lobby)
    {
        rooms.Add(id, lobby);
    }

    public YahtzeeHub(DatabaseContext context, UserRepository repository)
    {
        _context = context;
        _repository = repository;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        User? user = await GetUser();

        if (user is null || !users.ContainsKey(user.Id))
        {
            return;
        }

        YahtzeePlayer player = users[user.Id];

        rooms.TryGetValue(player.LobbyId, out YahtzeeLobby? lobby);

        if (lobby is null)
        {
            Context.Abort();

            return;
        }

        lobby.Players.Remove(player);

        if(lobby.Players.Count == 0)
        {
            await RemoveGame();
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task<bool> DoesRoomExist(uint roomId, uint userId)
    {
        if (!rooms.ContainsKey(roomId))
        {
            LobbyModel? lobby = await _context.Lobbies.Where(x => x.Id == roomId).FirstOrDefaultAsync();

            if(lobby is null)
            {
                return false;
            }

            rooms.Add(lobby.Id, new YahtzeeLobby(userId));
        }

        return true;
    }

    private async Task ForcedLeave(string reason)
    {
        await Clients.Caller.SendAsync("ForcedLeave", reason);
        Context.Abort();
    }

    public async Task<bool> Join(uint roomId)
    {
        User? user = await GetUser();

        if(user is null)
        {
            await ForcedLeave("Unauthorized");

            return false;
        }

        if (!await DoesRoomExist(roomId, (uint)user.Id))
        {
            await ForcedLeave("Room dose not exits");

            return false;
        }

        if (!users.ContainsKey(user.Id))
        {
            users.Add(user.Id, new YahtzeePlayer(user.Username, Context.ConnectionId, user.Id));
        }

        YahtzeePlayer player = users[user.Id];

        player.ConnectionId = Context.ConnectionId;

        if(roomId != player.LobbyId)
        {
            player.Restart(roomId);
        }

        /* If lobby dose exist update joined user ex. sync last state*/
        if(rooms.TryGetValue(roomId, out YahtzeeLobby? lobby))
        {
            player.LobbyId = roomId;

            if (!lobby.IsInitialized && lobby is not null)
            {
                if(player.UserId == lobby.CreatorId)
                {
                    lobby.Initialize(player);
                }

                await Clients.Caller.SendAsync("OnJoin", new YahtzeeOnJoinModel()
                {
                    Dices = player.Dices,
                    Points = player.Points,
                    SettedPoints = player.SettedPoints,
                    HasRound = true,
                    GameStarted = false,
                    IsCreator = true,
                    Players = lobby.GetPlayersModels()
                });
            }
            else if(lobby is not null)
            {
                if (!lobby.Players.Contains(player))
                {
                    lobby.Players.Add(player);
                }

                foreach (YahtzeePlayer lobbyPlayer in lobby.Players)
                {
                    if (lobby.GameStarted)
                    {
                        await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnJoin", new YahtzeeOnJoinModel()
                        {
                            Dices = lobbyPlayer.Dices,
                            Points = lobbyPlayer.Points,
                            SettedPoints = lobbyPlayer.SettedPoints,
                            HasRound = lobby.PlayerRound == lobbyPlayer,
                            GameStarted = lobby.GameStarted,
                            IsCreator = lobby.CreatorId == lobbyPlayer.UserId,
                            RollCount = lobbyPlayer.RollCount,
                            Players = lobby.GetPlayersModels(),
                        });
                    }
                    else
                    {
                        await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnJoin", new YahtzeeOnJoinModel()
                        {
                            GameStarted = lobby.GameStarted,
                            IsCreator = lobby.CreatorId == lobbyPlayer.UserId,
                            Players = lobby.GetPlayersModels(),
                        });
                    }
                }

                player.LobbyId = roomId;
            }
        }

        return true;
    }

    public async Task StartGame()
    {
        User? user = await GetUser();

        if (user is null)
        {
            Context.Abort();
            return;
        }

        YahtzeePlayer player = users[user.Id];

        YahtzeeLobby lobby = rooms[player.LobbyId];

        if (lobby.CreatorId == player.UserId && !lobby.GameStarted)
        {
            lobby.StartGame();

            if(lobby.PlayerRound is null)
            {
                throw new Exception("Lobby was started without PlayerRound");
            }

            YahtzeePlayerModel[] playerModels = lobby.GetPlayersModels();

            foreach (YahtzeePlayer lobbyPlayer in lobby.Players)
            {
                await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnJoin", new YahtzeeOnJoinModel()
                {
                    Dices = lobby.PlayerRound.Dices,
                    Points = lobbyPlayer.Points,
                    SettedPoints = lobbyPlayer.SettedPoints,
                    HasRound = lobby.PlayerRound == lobbyPlayer,
                    GameStarted = lobby.GameStarted,
                    IsCreator = lobby.CreatorId == lobbyPlayer.UserId,
                    Players = playerModels,
                    RollCount = lobbyPlayer.RollCount,
                });
            }
        }
    }

    public override async Task OnConnectedAsync()
    {
        User? user = await GetUser();

        if (user == null)
        {
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
    }

    public async Task OnRollDices(int[] dices)
    {
        User? user = await GetUser();

        if (user == null)
        {
            Context.Abort();

            return;
        }

        YahtzeePlayer player = users[user.Id];
        YahtzeeLobby lobby = rooms[player.LobbyId];

        if (!player.CanRoll)
        {
            return;
        }

        player.RollCount--;

        if(dices.Length == 0)
        {
            foreach(YahtzeeDice dice in player.Dices)
            {
                dice.Roll();
            }
        }

        for(int i = 0; i < player.Dices.Length; i++)
        {
            if (!dices.Contains(i))
            {
                player.Dices[i].Roll();
            }
        }

        foreach(YahtzeePlayer lobbyPlayer in lobby.Players)
        {
            await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("SetDices", users[user.Id].Dices);
        }
    }

    private async Task RemoveGame()
    {
        return;
        User? user = await GetUser();

        if(user == null)
        {
            return;
        }

        YahtzeePlayer player = users[user.Id];

        YahtzeeLobby lobby = rooms[player.LobbyId];

        foreach (YahtzeePlayer lobbyPlayer in users.Values.Where(x => x.LobbyId == player.LobbyId))
        {
            users.Remove(lobbyPlayer.UserId);
        }

        users.Remove(player.UserId);
        rooms.Remove(player.LobbyId);

        await DeleteRoom(player.LobbyId);
    }

    public async Task<YahtzeeOnPointSetModel> OnPointSet(YahtzeePointType pointType)
    {
        User? user = await GetUser();

        if (user == null)
        {
            return YahtzeeOnPointSetModel.UnSuccessFul;
        }

        YahtzeePlayer player = users[user.Id];

        YahtzeeLobby lobby = rooms[player.LobbyId];

        if (!lobby.GameStarted || lobby.PlayerRound != player)
        {
            return YahtzeeOnPointSetModel.UnSuccessFul;
        }

        var point = player.SetPointsFromPoint(pointType);

        if (!point.success)
        {
            return YahtzeeOnPointSetModel.UnSuccessFul;
        }

        lobby.NextRound();

        player.RollCount = 2;

        YahtzeePlayerModel[] playerModels = lobby.GetPlayersModels();

        foreach (YahtzeePlayer lobbyPlayer in lobby.Players)
        {
            await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnNextRound", new YahtzeeOnNexRoundModel()
            {
                HasRound = lobbyPlayer == lobby.PlayerRound,
                Players = playerModels,
                Dices = lobby.PlayerRound.Dices
            });
        }

        if (!player.HasMoves() && lobby.GetLastPlayer() == player)
        {
            await EndGame(lobby);
            Context.Abort();
        }

        return new YahtzeeOnPointSetModel()
        {
            IsSuccessFul = point.success,
            Points = player.Points,
            PointsFromPoint = point.points,
            Point = pointType,
        };
    }

    private async Task EndGame(YahtzeeLobby lobby)
    {
        YahtzeePlayer winner = lobby.Players.OrderBy(x => x.Points).First();

        foreach(YahtzeePlayer lobbyPlayer in lobby.Players)
        {
            YahtzeeEndGameModel endgameModel = new YahtzeeEndGameModel()
            {
                CoinsGotten = (float)(lobbyPlayer.Points * 0.1 * lobby.Players.Count),
                WinnerUsername = winner.PlayerName
            };

            await _repository.AddCoins(lobbyPlayer.UserId, endgameModel.CoinsGotten);

            await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnEndGame", endgameModel);
        }

        await RemoveGame();
    }

    private async Task<User?> GetUser()
    {
        Claim? userClaim = Context?.User?.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Jti).FirstOrDefault();

        if (userClaim is null || userClaim.Value is null)
        {
            return null;
        }

        return await _repository.GetByIdAsync(int.Parse(userClaim.Value));
    }
}