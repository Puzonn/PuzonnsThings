namespace PuzonnsThings.Models.Yahtzee;

public class YahtzeeLobby
{
    public readonly List<YahtzeePlayer> Players = new List<YahtzeePlayer>(4);

    public YahtzeePlayer? PlayerRound { get; set; }

    public readonly YahtzeePlayer Creator;

    public bool GameStarted { get; set; } = false;

    public void StartGame()
    {
        GameStarted = true;

        PlayerRound = Creator;

        foreach (YahtzeePlayer player in Players)
        {
            foreach (YahtzeeDice dice in player.Dices)
            {
                dice.Roll();
            }
        }
    }

    public YahtzeePlayer GetLastPlayer() => Players.Last();

    public YahtzeePlayerModel[] GetPlayersModels()
    {
        YahtzeePlayerModel[] players = new YahtzeePlayerModel[Players.Count];

        for (int i = 0; i < Players.Count; i++)
        {
            YahtzeePlayer lobbyPlayer = Players[i];

            players[i] = new YahtzeePlayerModel()
            {
                HasRound = PlayerRound == lobbyPlayer,
                PlayerName = lobbyPlayer.PlayerName,
                Points = lobbyPlayer.Points,
                SettedPoints = lobbyPlayer.SettedPoints.ToArray()
            };
        }

        return players;
    }

    public void NextRound()
    {
        if (!GameStarted || PlayerRound is null)
        {
            return;
        }

        int playerIndex = Players.IndexOf(PlayerRound);

        if (playerIndex + 1 == Players.Count)
        {
            PlayerRound = Players[0];
        }
        else
        {
            PlayerRound = Players[playerIndex + 1];
        }

        foreach(YahtzeePlayer player in Players)
        {
            foreach(YahtzeeDice dice in player.Dices)
            {
                dice.Roll();
            }
        }
    }

    public YahtzeeLobby(YahtzeePlayer creator)
    {
        Creator = creator;
    }
}