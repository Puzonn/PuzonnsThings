using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuzonnsThings.Databases;
using PuzonnsThings.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TodoApp.Repositories;

namespace Backend.Controllers;

[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly UserRepository _userRepository;

    public UsersController(DatabaseContext context, UserRepository repository)
    {
        _context = context;
        _userRepository = repository;
    }

    [HttpGet("api/[controller]/{id}")]
    public async Task<ActionResult<ApiUser?>> GetUser(int id)
    {
        User? user = await _userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        ApiUser apiUser = new ApiUser()
        {
            Coins = user.Coins,
            Id = user.Id,
            Username = user.Username
        };

        return apiUser;
    }

    [HttpGet("api/[controller]/self")]
    public async Task<ActionResult<ApiUser?>> GetSelf()
    {
        User? user = await GetUser();

        if (user is null)
        {
            return NotFound();
        }

        ApiUser apiUser = new ApiUser()
        {
            Coins = user.Coins,
            Id = user.Id,
            Username = user.Username
        };

        return Ok(apiUser);
    }

    private async Task<User?> GetUser()
    {
        Claim? userId = HttpContext.User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Jti).FirstOrDefault();

        if (userId is null || userId.Value is null)
        {
            return null;
        }

        return await _userRepository.GetByIdAsync(int.Parse(userId.Value));
    }
}