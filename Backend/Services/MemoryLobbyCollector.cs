using PuzonnsThings.Models;
using PuzonnsThings.Repositories;

namespace PuzonnsThings.Services;

public class MemoryLobbyCollector
{
    private readonly List<ILobbyCollectable> _lobbyCollectables = new List<ILobbyCollectable>();
    private readonly IServiceProvider serviceProvider;

    private const int CheckTime = 25;

    public MemoryLobbyCollector(IServiceProvider _serviceProvider)
    {
        Task.Run(RunCollector);
        serviceProvider = _serviceProvider;
    }

    private async Task RunCollector()
    {
        PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(CheckTime));

        while (await timer.WaitForNextTickAsync())
        {
            DateTime now = DateTime.UtcNow;
            List<ILobbyCollectable> collectedLobbies = new List<ILobbyCollectable>();

            foreach (var lobby in _lobbyCollectables)
            {
                if (lobby.ActivePlayers == 0)
                {
                    if (lobby.LastLobbySnapshot - now > TimeSpan.FromSeconds(CheckTime))
                    {
                        collectedLobbies.Add(lobby);
                    }
                }
            }

            if (collectedLobbies.Count > 0)
            {
                using (var scope = serviceProvider.CreateAsyncScope())
                {
                    var lobbyRepository = scope.ServiceProvider.GetService<LobbyRepository>();

                    if (lobbyRepository is null)
                    {
                        throw new Exception("Impossible dbContext null");
                    }

                    foreach (var lobby in collectedLobbies)
                    {
                        _lobbyCollectables.Remove(lobby);
                        await lobbyRepository.RemoveLobby(lobby.LobbyId);
                    }

                    await lobbyRepository.SaveChangesAsync();
                }
            }
        }
    }

    public void RegisterLobby(ILobbyCollectable lobby)
    {
        if (!_lobbyCollectables.Contains(lobby))
        {
            _lobbyCollectables.Add(lobby);
        }
    }
}