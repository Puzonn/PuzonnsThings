﻿using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PuzonnsThings.Models;
using PuzonnsThings.Models.Yahtzee;
using PuzonnsThings.Repositories;
using PuzonnsThings.Services;

namespace PuzonnsThings.Hubs.Yahtzee;

[Authorize]
public class YahtzeeHub : Hub
{
    private readonly IHubContext<YahtzeeHub> HubContext;
    private readonly MemoryLobbyCollector LobbyCollector;
    private readonly YahtzeeService LobbyService;
    private readonly IUserRepository Repository;
    private readonly YahtzeeHubContext YahtzeeContext;

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
    ///     Checks if room exist in memory, if room dose not exist it checks if room exist in database then creates room in
    ///     memory
    /// </summary>
    private async Task<bool> DoesRoomExist(uint roomId, uint userId)
    {
        if (!LobbyService.HasLobby(roomId))
        {
            var lobby = await LobbyService.GetLobbyModel(roomId);

            if (lobby is null) return false;

            LobbyService.AddRoom(lobby.Id, new YahtzeeLobby(HubContext, userId, roomId));
        }
        else /* Check if lobby should be recreated */
        {
            var lobby = LobbyService.GetLobby(roomId);

            if (lobby is null) return false;

            if (lobby.GameEnded) await ForcedLeave("Game ended");
        }

        return true;
    }

    private async Task ForcedLeave(string reason)
    {
        await Clients.Caller.SendAsync("ForcedLeave", reason);
        Context.Abort();
    }

    /// <summary>
    ///     Handles user joins into lobby
    /// </summary>
    /// <param name="roomId">Id of the room</param>
    /// <returns>True if user is authorized and rooms exists</returns>
    public async Task<bool> Join(uint roomId)
    {
        var user = await GetUser();

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
            LobbyService.AddPlayer(user.Id,
                new YahtzeePlayer(user.Username, user.Avatar, Context.ConnectionId, user.Id));

        var player = LobbyService.GetPlayer(user.Id);

        player.ConnectionId = Context.ConnectionId;

        if (roomId != player.ConnectedLobbyId) player.Restart(roomId);

        /* If lobby dose exist update joined user ex. sync last state*/
        if (LobbyService.HasLobby(roomId, out var lobby))
        {
            if (lobby is null) return false;

            LobbyCollector.RegisterLobby(lobby);

            player.ConnectedLobbyId = roomId;

            if (!lobby.IsInitialized)
            {
                if (player.UserId == lobby.LobbyCreatorId) lobby.Initialize(player);

                await Clients.Caller.SendAsync("OnJoin", new YahtzeeOnJoinModel
                {
                    Dices = player.Dices,
                    Points = player.Points,
                    PlacedPoints = player.PlacedPoints,
                    HasRound = true,
                    GameStarted = false,
                    IsCreator = true,
                    Players = lobby.GetPlayersModels()
                });
            }
            else
            {
                if (!lobby.Players.Contains(player)) lobby.Players.Add(player);

                foreach (var lobbyPlayer in lobby.Players)
                    if (lobby.GameStarted)
                        await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnJoin", new YahtzeeOnJoinModel
                        {
                            Dices = lobbyPlayer.Dices,
                            Points = lobbyPlayer.Points,
                            PlacedPoints = lobbyPlayer.PlacedPoints,
                            HasRound = lobby.PlayerRound == lobbyPlayer,
                            GameStarted = lobby.GameStarted,
                            IsCreator = lobby.LobbyCreatorId == lobbyPlayer.UserId,
                            RollCount = lobbyPlayer.RollCount,
                            Players = lobby.GetPlayersModels(),
                            Options = lobby.LobbyOptions
                        });
                    else
                        await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnJoin", new YahtzeeOnJoinModel
                        {
                            GameStarted = lobby.GameStarted,
                            IsCreator = lobby.LobbyCreatorId == lobbyPlayer.UserId,
                            Players = lobby.GetPlayersModels(),
                            Options = lobby.LobbyOptions,
                            StartState = lobby.IsReadyToStart()
                        });

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

        if (lobby.LobbyCreatorId == player.UserId && !lobby.GameStarted)
        {
            if (!lobby.StartGame()) return;

            var playerModels = lobby.GetPlayersModels();

            foreach (var lobbyPlayer in lobby.Players)
                await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnJoin", new YahtzeeOnJoinModel
                {
                    Dices = lobby.PlayerRound!.Dices,
                    Points = lobbyPlayer.Points,
                    PlacedPoints = lobbyPlayer.PlacedPoints,
                    HasRound = lobby.PlayerRound == lobbyPlayer,
                    GameStarted = lobby.GameStarted,
                    IsCreator = lobby.LobbyCreatorId == lobbyPlayer.UserId,
                    Players = playerModels,
                    RollCount = lobbyPlayer.RollCount
                });
        }
    }

    public override async Task OnConnectedAsync()
    {
        var user = await GetUser();

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

        var playerLobbyPlace = lobby.GetPlayerLobbyPlace(player.UserId);
        var synchronize = false;

        if (lobby.LobbyCreatorId == player.UserId)
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
            var choosePlaceEvent = new YahtzeeOnChoosePlace
            {
                Players = lobby.GetPlayersModels(),
                StartState = false
            };

            foreach (var lobbyPlayer in lobby.Players)
                await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("ChoosePlaceStateCallback", choosePlaceEvent);
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
            var choosePlaceEvent = new YahtzeeOnChoosePlace
            {
                Players = lobby.GetPlayersModels(),
                StartState = lobby.IsReadyToStart()
            };

            foreach (var lobbyPlayer in lobby.Players)
                await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("ChoosePlaceStateCallback", choosePlaceEvent);
        }
    }

    public async Task OnOptionsPrivacyChange(bool state)
    {
        var context = YahtzeeContext.GetInfo(await GetUser());
        var lobby = context.lobby;
        var player = context.player;

        if (lobby is null || player is null)
        {
            await ForcedLeave("OnOptionsPrivacyChange null value");
            return;
        }

        if (player.UserId == lobby.LobbyCreatorId)
        {
            lobby.ChangePrivacy(state);

            foreach (var lobbyPlayer in lobby.Players)
                await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("ChangePrivacyCallback", state);
        }
    }

    public async Task OnOptionsGameTimeChange(int gameTime)
    {
        var context = YahtzeeContext.GetInfo(await GetUser());
        var lobby = context.lobby;
        var player = context.player;

        if (lobby is null || player is null)
        {
            await ForcedLeave("OnOptionsGameTimeChange null value");
            return;
        }

        if (player.UserId == lobby.LobbyCreatorId && lobby.ChangeGameTime(gameTime))
            foreach (var lobbyPlayer in lobby.Players)
                await Clients.Client(lobbyPlayer.ConnectionId)
                    .SendAsync("ChangeGameTimeStateCallback", lobby.LobbyOptions.GameTime);
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

        if (player.UserId == lobby.LobbyCreatorId)
        {
            lobby.ChangeMaxPlayers(maxPlayersCount);

            var status = new YahtzeeOptionsMaxPlayerChange
            {
                MaxPlayersState = maxPlayersCount,
                Players = lobby.GetPlayersModels()
            };

            foreach (var lobbyPlayer in lobby.Players)
                await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("ChangeMaxPlayersStateCallback", status);
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

        if (!player.CanRoll || lobby is null) return;

        player.RollCount--;

        if (dices.Length == 0)
            foreach (var dice in player.Dices)
                dice.Roll();

        for (var i = 0; i < player.Dices.Length; i++)
            if (!dices.Contains(i))
                player.Dices[i].Roll();

        foreach (var lobbyPlayer in lobby.Players)
            await Clients.Client(lobbyPlayer.ConnectionId)
                .SendAsync("SetDices", LobbyService.GetPlayer(player.UserId).Dices);
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

        if (!lobby.GameStarted || lobby.PlayerRound != player) return YahtzeeOnPointSetModel.UnSuccessFul;

        var point = player.SetPointsFromPoint(pointType);

        if (!point.success) return YahtzeeOnPointSetModel.UnSuccessFul;

        await lobby.NextRound();

        var playerModels = lobby.GetPlayersModels();

        foreach (var lobbyPlayer in lobby.Players)
            await Clients.Client(lobbyPlayer.ConnectionId).SendAsync("OnNextRound", new YahtzeeOnNextRoundModel
            {
                HasRound = lobbyPlayer == lobby.PlayerRound,
                Players = playerModels,
                Dices = lobby.PlayerRound.Dices
            });

        return new YahtzeeOnPointSetModel
        {
            IsSuccessFul = point.success,
            Points = player.Points,
            PointsFromPoint = point.points,
            Point = pointType
        };
    }

    private async Task<User?> GetUser()
    {
        var userClaim = (Context?.User?.Claims).FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti);

        if (userClaim is null || userClaim.Value is null) return null;

        return await Repository.GetByIdAsync(int.Parse(userClaim.Value));
    }
}