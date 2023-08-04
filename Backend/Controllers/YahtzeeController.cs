using PuzonnsThings.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuzonnsThings.Databases;
using PuzonnsThings.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PuzonnsThings.Controllers;

[Authorize]
[ApiController]
public class YahtzeeController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly IUserRepository _repository;
    private readonly LobbyRepository _lobbyRepository;

    public YahtzeeController(DatabaseContext context, IUserRepository repository, LobbyRepository lobbyRepository)
    {
        _context = context;
        _repository = repository;
        _lobbyRepository = lobbyRepository;
    }

    private async Task<User?> GetUser()
    {
        Claim? userId = HttpContext.User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Jti).FirstOrDefault();

        if (userId is null || userId.Value is null)
        {
            return null;
        }

        return await _repository.GetByIdAsync(int.Parse(userId.Value));
    }
}