﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuzonnsThings.Databases;
using PuzonnsThings.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PuzonnsThings.Repositories;
using Microsoft.AspNetCore.Identity;

namespace PuzonnsThings.Controllers;

[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository repository)
    {
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
            Coins = user.Balance,
            Avatar = user.Avatar,
            UserId = user.Id,
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
            Coins = user.Balance,
            UserId = user.Id,
            Avatar = user.Avatar,
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