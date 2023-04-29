using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PuzonnsThings.Databases;
using PuzonnsThings.Models;
using PuzonnsThings.Models.Auth;

namespace PuzonnsThings.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(DatabaseContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("/api/[controller]/register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var user = await _context.Users.Where(x => x.Username == model.Username).FirstOrDefaultAsync();

        if (user is null)
        {
            User newUser = new User(model.Username, HashPassword(model.Password), model.Email);

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return Ok();
        }
        else
        {
            return Problem("User dose exists");
        }
    }

    [AllowAnonymous]
    [HttpPost("/api/[controller]/login")]
    public async Task<IActionResult> Login([FromBody] LoginModel login)
    {
        var user = await _context.Users.Where(x => x.Username == login.Username).FirstOrDefaultAsync();

        if (user is null)
        {
            return BadRequest("Wrong login validation");
        }

        if (!VerifyPassword(login.Password, user.Password))
        {
            return BadRequest("Wrong login validation");
        }

        string sessionId = GenerateJwtToken(user);

#if DEBUG
        string CookieDomain = "localhost";
#else
        string CookieDomain = "puzonnsthings.pl";
#endif
        var cookieOption = new CookieOptions()
        {
            HttpOnly = false,
            IsEssential = false,
            Secure = false,
            Path = "/",
            Domain = CookieDomain,
            Expires = DateTime.UtcNow.AddDays(1)
        };

        Response.Cookies.Append("Bearer", sessionId, cookieOption);
        Response.Cookies.Append("Username", user.Username, cookieOption);

        return Ok();
    }

    [Authorize]
    [HttpGet("/api/[controller]/validate")]
    public IActionResult Validate()
    {
        return Ok();
    }

    [HttpPost("api/[controller]/logout")]
    public async Task<IActionResult> Logout()
    {
        return Ok();
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
                new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString())
        };
        var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials);


        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    private bool VerifyPassword(string password, string hashed) => BCrypt.Net.BCrypt.Verify(password, hashed);
}