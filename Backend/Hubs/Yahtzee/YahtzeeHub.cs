using PuzonnsThings.Models.Lobbies;
using PuzonnsThings.Models.Yahtzee;
using PuzonnsThings.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PuzonnsThings.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PuzonnsThings.Repositories;

namespace PuzonnsThings.Hubs.Yahtzee;

[Authorize]
public class YahtzeeHub : Hub
{
    private readonly YahtzeeService LobbyService;
    private readonly IUserRepository Repository;
    private readonly MemoryLobbyCollector LobbyCollector;
    private readonly YahtzeeHubContext YahtzeeContext;

    private readonly IHubContext<YahtzeeHub> HubContext;

    public YahtzeeHub(IUserRepository repository, YahtzeeService lobbyService,
        MemoryLobbyCollector lobbyCollector, IHubContext<YahtzeeHub> hubContext)
    {
        HubContext = hubContext;
        LobbyService = lobbyService;
        Repository = repository;
        LobbyCollector = lobbyCollector;

        YahtzeeContext = new YahtzeeHubContext(LobbyService);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var context = YahtzeeContext.GetInfo(await GetUser());
        var player = context.player;
        var lobby = context.lobby;

        if (lobby is null || player is null)
        {
            await ForcedLeave("Impossible null value");

            return;
        }

        lobby.Players.Remove(player);

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Checks if room exist in memory, if room dose not exist it checks if room exist in database then creates room in memory
    /// </summary>
    private async Task<bool> DoesRoomExist(uint roomId, uint userId)
    {
        if (!LobbyService.HasLobby(roomId))
        {
            LobbyModel? lobby = await LobbyService.GetLobbyModel(roomId);

            if (lobby is null)
            {
                return false;
            }

            LobbyService.AddRoom(lobby.Id, new YahtzeeLobby(HubContext, userId, roomId));
        }
        else /* Check if lobby should be recreated */
        {
            YahtzeeLobby? lobby = LobbyService.GetLobby(roomId);

            if (lobby is null)
            {
                return false;
            }

            if (lobby.GameEnded)
            {
                await ForcedLeave("Game ended");
            }
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

        if (user is null)
        {
            await ForcedLeave("Unauthorized");

            return false;
        }

        if (!await DoesRoomExist(roomId, (uint)user.Id))
        {
            await ForcedLeave("Room dose not exits");

            return false;
        }

        if (!LobbyService.HasPlayer(user.Id))
        {
            LobbyService.AddPlayer(user.Id, new YahtzeePlayer(user.Username, Context.ConnectionId, user.Id));
        }

        YahtzeePlayer player = LobbyService.GetPlayer(user.Id);

        player.ConnectionId = Context.ConnectionId;

        if (roomId != player.ConnectedLobbyId)
        {
            player.Restart(roomId);
        }

        /* If lobby dose exist update joined user ex. sync last state*/
        if (LobbyService.HasLobby(roomId, out YahtzeeLobby? lobby))
        {
            if (lobby is null)
            {
                return false;
            }

            LobbyCollector.RegisterLobby(lobby);

            player.ConnectedLobbyId = roomId;

            if (!lobby.IsInitialized && lobby is not null)
            {
                if (player.UserId == lobby.CreatorId)
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
            else if (lobby is not null)
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
                            Options = lobby.Options,
                        });
                    }
                    else
                    {
                        await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnJoin", new YahtzeeOnJoinModel()
                        {
                            GameStarted = lobby.GameStarted,
                            IsCreator = lobby.CreatorId == lobbyPlayer.UserId,
                            Players = lobby.GetPlayersModels(),
                            Options = lobby.Options,
                            StartState = lobby.IsReadyToStart()
                        });
                    }
                }

                player.ConnectedLobbyId = roomId;
            }
        }

        return true;
    }

    public async Task StartGame()
    {
        var context = YahtzeeContext.GetInfo(await GetUser());
        var player = context.player;
        var lobby = context.lobby;

        if (lobby is null || player is null)
        {
            await ForcedLeave("StartGame null value");

            return;
        }

        if (lobby.CreatorId == player.UserId && !lobby.GameStarted)
        {
            if (!lobby.StartGame())
            {
                return;
            }

            YahtzeePlayerModel[] playerModels = lobby.GetPlayersModels();

            foreach (YahtzeePlayer lobbyPlayer in lobby.Players)
            {
                await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnJoin", new YahtzeeOnJoinModel()
                {
                    Dices = lobby.PlayerRound!.Dices,
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

    public async Task OnLobbyPlaceClick(int placeId)
    {
        var context = YahtzeeContext.GetInfo(await GetUser());
        var player = context.player;
        var lobby = context.lobby;

        if (lobby is null || player is null)
        {
            await ForcedLeave("OnPointSet null value");

            return;
        }

        int playerLobbyPlace = lobby.GetPlayerLobbyPlace(player.UserId);
        bool synchronize = false;

        if (lobby.CreatorId == player.UserId)
        {
            synchronize = true;
            lobby.RemovePlayerFromLobbyPlace(player.UserId);
        }
        else if (playerLobbyPlace == placeId)
        {
            synchronize = true;
            lobby.RemovePlayerFromLobbyPlace(player.UserId);
        }

        if (synchronize)
        {
            YahtzeeOnChoosePlace choosePlaceEvent = new YahtzeeOnChoosePlace()
            {
                Players = lobby.GetPlayersModels(),
                StartState = false,
            };

            foreach (YahtzeePlayer lobbyPlayer in lobby.Players)
            {
                await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("ChoosePlaceStateCallback", choosePlaceEvent);
            }
        }
    }

    public async Task OnChoosePlaceState(int placeId)
    {
        var context = YahtzeeContext.GetInfo(await GetUser());
        var lobby = context.lobby;
        var player = context.player;

        if (lobby is null || player is null)
        {
            await ForcedLeave("OnChoosePlaceState null value");
            return;
        }

        if (lobby.ChoosePlace(player.UserId, placeId))
        {
            YahtzeeOnChoosePlace choosePlaceEvent = new YahtzeeOnChoosePlace()
            {
                Players = lobby.GetPlayersModels(),
                StartState = lobby.IsReadyToStart()
            };

            foreach (YahtzeePlayer lobbyPlayer in lobby.Players)
            {
                await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("ChoosePlaceStateCallback", choosePlaceEvent);
            }
        }
    }

    public async Task OnOptionsMaxPlayersChange(int maxPlayersCount)
    {
        var context = YahtzeeContext.GetInfo(await GetUser());
        var lobby = context.lobby;
        var player = context.player;

        if (lobby is null || player is null)
        {
            await ForcedLeave("OnOptionsMaxPlayersChange null value");
            return;
        }

        if (player.UserId == lobby.CreatorId)
        {
            lobby.ChangeMaxPlayers(maxPlayersCount);

            YahtzeeOptionsMaxPlayerChange status = new YahtzeeOptionsMaxPlayerChange()
            {
                MaxPlayersState = maxPlayersCount,
                Players = lobby.GetPlayersModels()
            };

            foreach (YahtzeePlayer lobbyPlayer in lobby.Players)
            {
                await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("ChangeMaxPlayersStateCallback", status);
            }
        }
    }

    public async Task OnRollDices(int[] dices)
    {
        var context = YahtzeeContext.GetInfo(await GetUser());
        var lobby = context.lobby;
        var player = context.player;

        if (lobby is null || player is null)
        {
            await ForcedLeave("OnRollDices null value");
            return;
        }

        if (!player.CanRoll || lobby is null)
        {
            return;
        }

        player.RollCount--;

        if (dices.Length == 0)
        {
            foreach (YahtzeeDice dice in player.Dices)
            {
                dice.Roll();
            }
        }

        for (int i = 0; i < player.Dices.Length; i++)
        {
            if (!dices.Contains(i))
            {
                player.Dices[i].Roll();
            }
        }

        foreach (YahtzeePlayer lobbyPlayer in lobby.Players)
        {
            await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("SetDices", LobbyService.GetPlayer(player.UserId).Dices);
        }
    }

    public async Task<YahtzeeOnPointSetModel> OnPointSet(YahtzeePointType pointType)
    {
        var context = YahtzeeContext.GetInfo(await GetUser());
        var lobby = context.lobby;
        var player = context.player;

        if (lobby is null || player is null)
        {
            await ForcedLeave("OnPointSet null value");

            return YahtzeeOnPointSetModel.UnSuccessFul;
        }

        if (!lobby.GameStarted || lobby.PlayerRound != player)
        {
            return YahtzeeOnPointSetModel.UnSuccessFul;
        }

        var point = player.SetPointsFromPoint(pointType);

        if (!point.success)
        {
            return YahtzeeOnPointSetModel.UnSuccessFul;
        }

        await lobby.NextRound();

        YahtzeePlayerModel[] playerModels = lobby.GetPlayersModels();

        foreach (YahtzeePlayer lobbyPlayer in lobby.Players)
        {
            await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnNextRound", new YahtzeeOnNextRoundModel()
            {
                HasRound = lobbyPlayer == lobby.PlayerRound,
                Players = playerModels,
                Dices = lobby.PlayerRound.Dices
            });
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

        return await Repository.GetByIdAsync(int.Parse(userClaim.Value));
    }
}