using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PuzonnsThings.Databases;
using PuzonnsThings.Models;
using PuzonnsThings.Models.Yahtzee;
using TodoApp.Repositories;

namespace PuzonnsThings.Controllers;

[Authorize]
[ApiController]
public class YahtzeeController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly UserRepository _respository;

    public YahtzeeController(DatabaseContext context, UserRepository respository)
    {
        _context = context;
        _respository = respository;
    }

    [HttpPost("/api/[controller]/create")]
    public async Task<ActionResult<YahtzeeRoomModel>> CreateRoom()
    {
        User? user = await GetUser();

        if (user is null)
        {
            return Forbid("User is not logged in");
        }

        if (_context.YahtzeeRooms.Where(x => x.CreatorId == user.Id).Any())
        {
            return BadRequest("Room with given id already exist");
        }

        YahtzeeRoomModel room = new YahtzeeRoomModel(user.Username, user.Id);

        await _context.YahtzeeRooms.AddAsync(room);

        await _context.SaveChangesAsync();

        return Ok(room);
    }

    [HttpGet("/api/[controller]/fetch")]
    public async Task<IActionResult> Fetch()
    {
        User? user = await GetUser();

        if (user is null)
        {
            return Forbid("User is not logged in");
        }

        return Ok(_context.YahtzeeRooms.Take(10));
    }

    private async Task<User?> GetUser()
    {
        Claim? userId = HttpContext.User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Jti).FirstOrDefault();

        if (userId is null || userId.Value is null)
        {
            return null;
        }

        return await _respository.GetByIdAsync(int.Parse(userId.Value));
    }
}