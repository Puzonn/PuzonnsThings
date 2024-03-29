﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PuzonnsThings.Databases;
using PuzonnsThings.Models;
using PuzonnsThings.Models.WatchTogether;
using PuzonnsThings.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PuzonnsThings.Repositories;

namespace PuzonnsThings.Hubs.WatchTogether;

[Authorize]
public class WatchTogetherHub : Hub
{
    private readonly WatchTogetherService _watchTogetherService;
    private readonly DatabaseContext _dbContext;
    private readonly UserRepository _repository;

    private readonly object[] emptyArgs = new object[0];

    public WatchTogetherHub(DatabaseContext context, WatchTogetherService watchTogetherService, UserRepository respository)
    {
        _dbContext = context;
        _watchTogetherService = watchTogetherService;
        _repository = respository;
    }

    public async Task JoinRoom(int roomId)
    {
        /*
        WatchTogetherRoomModel? room = _dbContext.WatchTogetherRooms.Where(x => x.Id == roomId).FirstOrDefault();

        User? user = await GetUser();

        if (room is null || user is null)
        {
            Context.Abort();

            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());

        if (user.Id == room.CreatorId)
        {
            if (_watchTogetherService.GetByCreator(user.Id) == null)
            {
                _watchTogetherService.AddCachedRoom(new WatchTogetherConnectionRoomCache(room.Id, user.Id, Context.ConnectionId));
            }
            else
            {
                _watchTogetherService.UpdateConnectionId(room.Id, Context.ConnectionId);
            }

            await Clients.Caller.SendCoreAsync("CreatorJoined", emptyArgs);

            return;
        }
        else
        {
            string? creatorConnection = _watchTogetherService.GetByRoomId(room.Id)?.CreatorConnectionId;

            if (string.IsNullOrEmpty(creatorConnection))
            {
                Context.Abort();
                return;
            }

            WatchTogetherSyncModel syncModel = await Clients.Client(creatorConnection).InvokeCoreAsync<WatchTogetherSyncModel>("GetInfo", emptyArgs, default);

            await Clients.Caller.SendAsync("SyncJoinState", syncModel);
        }
        */
    }

    public async Task SyncState(WatchTogetherSyncStateModel model)
    {
        WatchTogetherConnectionRoomCache? room = await GetRoom();

        if (room is null)
        {
            Context.Abort();
            return;
        }

        await Clients.Group(room.GroupId.ToString()).SendAsync("ChangeState", model);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    public override async Task OnConnectedAsync()
    {
        User? user = await GetUser();

        if (user is null)
        {
            Context?.Abort();
        }
    }

    private async Task<WatchTogetherConnectionRoomCache?> GetRoom()
    {
        User? user = await GetUser();

        if (user is null)
        {
            Context.Abort();
            return null;
        }

        return _watchTogetherService.GetByMember(user.Id);
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