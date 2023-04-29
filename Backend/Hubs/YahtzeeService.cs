using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PuzonnsThings.Models.Yahtzee;
using PuzonnsThings.Databases;
using PuzonnsThings.Models;
using TodoApp.Repositories;

namespace PuzonnsThings.Hubs;

[Authorize]
public class YahtzeeService : Hub
{
    private static readonly Dictionary<int, YahtzeePlayer> users = new Dictionary<int, YahtzeePlayer>();
    private static readonly Dictionary<int, YahtzeeLobby> rooms = new Dictionary<int, YahtzeeLobby>();

    private readonly DatabaseContext _context;
    private readonly UserRepository _respository;

    public YahtzeeService(DatabaseContext context, UserRepository respository)
    {
        _context = context;
        _respository = respository;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        User? user = await GetUser();

        if(user is null)
        {
            return;
        }

        YahtzeePlayer player = users[user.Id];

        YahtzeeLobby lobby = rooms[player.LobbyId];

        lobby.Players.Remove(player);

        if(lobby.Players.Count == 0)
        {
            await RemoveGame();
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<bool> Join(int roomId)
    {
        User? user = await GetUser();

        if (user is null)
        {
            return false;
        }

        if (!users.ContainsKey(user.Id))
        {
            users.Add(user.Id, new YahtzeePlayer(user.Username, Context.ConnectionId, user.Id));
        }

        YahtzeePlayer player = users[user.Id];

        player.ConnectionId = Context.ConnectionId;
        
        if (rooms.TryGetValue(roomId, out YahtzeeLobby? lobby))
        {
            if (lobby is null)
            {
                return false;
            }

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
                        IsCreator = lobby.Creator == lobbyPlayer,
                        RollCount = lobbyPlayer.RollCount,
                        Players = lobby.GetPlayersModels(),
                    });
                }
                else
                {
                    await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnJoin", new YahtzeeOnJoinModel()
                    {
                        GameStarted = lobby.GameStarted,
                        IsCreator = lobby.Creator == lobbyPlayer,
                        Players = lobby.GetPlayersModels(),
                    });
                }
            }

            player.LobbyId = roomId;
        }
        else
        {
            YahtzeeRoomModel? room = _context.YahtzeeRooms.Where(x => x.Id == roomId).FirstOrDefault();

            if (room is null)
            {
                return false;
            }

            YahtzeeLobby newLobby = new YahtzeeLobby(player)
            {
                PlayerRound = player
            };

            newLobby.Players.Add(player);

            rooms.Add(room.Id, newLobby);

            await Clients.Caller.SendAsync("OnJoin", new YahtzeeOnJoinModel()
            {
                Dices = player.Dices,
                Points = player.Points,
                SettedPoints = player.SettedPoints,
                HasRound = true,
                GameStarted = false,
                IsCreator = true,
                Players = newLobby.GetPlayersModels()
            });

            player.LobbyId = roomId;
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

        if (lobby.Creator == player && !lobby.GameStarted)
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
                    IsCreator = lobby.Creator == lobbyPlayer,
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

        _context.YahtzeeRooms.Remove(_context.YahtzeeRooms.Where(x => x.Id == player.LobbyId).First());
        await _context.SaveChangesAsync();
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
            foreach(YahtzeePlayer lobbyPlayer in lobby.Players)
            {
                await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnEndGame");

                await RemoveGame();
            }

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

    private async Task<User?> GetUser()
    {
        Claim? userClaim = Context?.User?.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Jti).FirstOrDefault();

        if (userClaim is null || userClaim.Value is null)
        {
            return null;
        }

        return await _respository.GetByIdAsync(int.Parse(userClaim.Value));
    }
}