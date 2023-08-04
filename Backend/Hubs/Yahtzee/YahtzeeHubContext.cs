using PuzonnsThings.Services;
using PuzonnsThings.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PuzonnsThings.Hubs.Yahtzee;

public class YahtzeeHubContext
{
    private readonly YahtzeeService LobbyService;

    public YahtzeeHubContext(YahtzeeService lobbyService)
    {
        LobbyService = lobbyService;
    }

    public (YahtzeeLobby? lobby, YahtzeePlayer? player) GetInfo(User? user)
    {
        if (user is null)
        {
            return (null, null);
        }

        YahtzeePlayer player = LobbyService.GetPlayer(user.Id);

        YahtzeeLobby? lobby = LobbyService.GetLobby(player.ConnectedLobbyId);

        return (lobby, player);
    }
}