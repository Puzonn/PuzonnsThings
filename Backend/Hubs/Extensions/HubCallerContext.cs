using Microsoft.AspNetCore.SignalR;
using PuzonnsThings.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TodoApp.Repositories;

namespace Backend.Hubs.Extensions;

public static class HubUserContext
{
    public static async Task<User> GetUser(this HubCallerContext context, IUserRepository userRepository)
    {
        Claim? userClaim = context.User?.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Jti).FirstOrDefault();

        if (userClaim is null)
        {
            throw new Exception("User claim was null in authorized context");
        }

        User? user = await userRepository.GetByIdAsync(int.Parse(userClaim.Value));

        if (user is null)
        {
            throw new Exception("User was null in authorized context");
        }

        return user;
    }
}
