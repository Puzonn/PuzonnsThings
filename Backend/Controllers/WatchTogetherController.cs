using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PuzonnsThings.Databases;
using PuzonnsThings.Models;
using PuzonnsThings.Models.WatchTogether;
using PuzonnsThings.Services;
using TodoApp.Repositories;

namespace PuzonnsThings.Controllers;

[Authorize]
[ApiController]
public class WatchTogetherController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly WatchTogetherService watchTogetherService;
    private readonly UserRepository _respository;

    public WatchTogetherController(DatabaseContext context, WatchTogetherService service, UserRepository respository)
    {
        _context = context;
        watchTogetherService = service;
        _respository = respository;
    }

    [HttpPost("/api/[controller]/create")]
    public async Task<IActionResult> CreateRoom([FromBody] RoomCreateModel createModel)
    {
        User? user = await GetUser();

        if (user is null)
        {
            return Forbid("User is not logged in");
        }

        await watchTogetherService.CreateRoom(createModel, user);

        return Ok();
    }

    [HttpPost("/api/[controller]/join")]
    public async Task<ActionResult> JoinRoom([FromQuery] int id)
    {
        User? user = await GetUser();

        if (user is null)
        {
            return Forbid("User is not logged in");
        }

        bool doseExist = _context.WatchTogetherRooms.Where(x => x.Id == id).FirstOrDefault() != null;

        if (!doseExist)
        {
            return BadRequest("Room with given id dose not exist");
        }

        return Ok();
    }

    [HttpGet("/api/[controller]/get")]
    public async Task<ActionResult> GetRoom([FromQuery] int id)
    {
        User? user = await GetUser();

        if (user is null)
        {
            return Forbid();
        }

        WatchTogetherRoomApiModel? room = await watchTogetherService.GetRoom(id);

        if (room is null)
        {
            return BadRequest("Room with given id dose not exist");
        }

        return Ok(room);
    }

    [AllowAnonymous]
    [HttpGet("/api/[controller]/fetch")]
    public async Task<ActionResult<WatchTogetherRoomApiModel[]>> FetchRooms()
    {
        return Ok(await watchTogetherService.FetchRooms(10));
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