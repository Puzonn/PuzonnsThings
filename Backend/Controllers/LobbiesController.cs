using Backend.Models.Lobbies;
using Backend.Repositories;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Mvc;
using PuzonnsThings.Hubs;
using PuzonnsThings.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TodoApp.Repositories;
using PuzonnsThings.Models.Yahtzee;

namespace Backend.Controllers;

[Authorize]
[ApiController]
public class LobbiesController : ControllerBase
{
    private readonly LobbyRepository repository;
    private readonly UserRepository userRepository;

    public LobbiesController(LobbyRepository _repository, UserRepository _userRepository)
    {
        repository = _repository;
        userRepository = _userRepository;
    }

    [HttpGet("/api/[controller]/fetch/{lobbyType}")]
    public Task<LobbyModel[]> FetchLobbies(string lobbyType)
    {
        if (LobbyTypes.Yahtzee.ApiName == lobbyType)
        {
            return repository.FetchLobbies(LobbyTypes.Yahtzee);
        }
        else
        {
            return repository.FetchLobbies(LobbyTypes.WatchTogether);
        }
    }

    [HttpPost("/api/[controller]/create/{lobbyType}")]
    public async Task<ActionResult<LobbyModel>> CreateLobby(string lobbyType, [FromBody] object? lobbyData)
    {
        User? user = await GetUser();

        if(user is null)
        {
            return Forbid("User is not logged in");
        }

        if(lobbyType == LobbyTypes.Yahtzee.ApiName)
        {
            LobbyModel model = new LobbyModel()
            {
                LobbyType = LobbyTypes.Yahtzee.Name,
                MaxPlayersCount = LobbyTypes.Yahtzee.MaxPlayerCount,
                CreatorUserId = user.Id,
                LobbyCreator = user.Username,
                PlayersCount = 1,
                Status = LobbyStatus.Waiting
            };

            LobbyModel lobby = await repository.AddLobby(model);

            return lobby;
        }

        return BadRequest();
    }

    [HttpGet("/api/[controller]/fetch")]
    public Task<LobbyModel[]> FetchAllLobbies()
    {
        return repository.FetchAllLobbies();
    }


    private async Task<User?> GetUser()
    {
        Claim? userId = HttpContext.User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Jti).FirstOrDefault();

        if (userId is null || userId.Value is null)
        {
            return null;
        }

        return await userRepository.GetByIdAsync(int.Parse(userId.Value));
    }
}
