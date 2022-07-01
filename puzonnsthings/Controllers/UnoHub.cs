using Microsoft.AspNetCore.SignalR;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace puzonnsthings.Controllers;

public class UnoHub : Hub
{
    private static readonly UnoGameLobby uno = new();

    public async Task PlayerJoinned(string username, string connectionID)
    {
        UnoGamePlayer? player = uno.Players.Find(x => x.PlayerName == username);

        if (player == null)
        {
            uno.AddPlayer(connectionID, username,uno);
        }
        else
        {
            player.ConnectionID = connectionID;
        }
        await Clients.All.SendAsync("GameInfoCallback", uno.GetInfo());
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        UnoGamePlayer? player = uno.Players.Find(x => x.ConnectionID == Context.ConnectionId);
        uno.RemovePlayer(player);

        Clients.All.SendAsync("GameInfoCallback", uno.GetInfo());
        return base.OnDisconnectedAsync(exception);
    }

    public async Task GetNewCard(string ConnectionID)
    {
        UnoGamePlayer? player = uno.Players.Find(x => x.ConnectionID == ConnectionID);
        if (player != null)
        {
            player.AddCard();
            await Clients.Client(ConnectionID).SendAsync("SetCards", JsonSerializer.Serialize(player.Cards));
        }
        await ChangeTurn(uno,player);
    }

    public async Task RegisterCard(string ConnectionID, UnoGameCard? card)
    {
        UnoGamePlayer? player = uno.Players.Find(x=> x.ConnectionID == ConnectionID);

        if(uno.PlayerTurn.ConnectionID != ConnectionID)
        {
            Console.WriteLine("NOT_PLAYER_TURN");
            return;
        }

        if(player is not null && card is not null)
        {
            if(UnoRuleset.IsValidCard(card,uno.CardOnTop))
            {
                player.RemoveCard(card);
                uno.SetCardOnTop(card);

                if(card.Power >= 10)
                {
                    UnoRuleset.RegisterActionCard(Clients,card,uno);
                }

                ChangeTurn(uno,player);
            }     
        }
    }

    public async Task ChangeTurn(UnoGameLobby lobby,UnoGamePlayer player)
    {
        UnoGamePlayer NextPlayer = lobby.GetNextPlayer(player.ConnectionID);
        lobby.PlayerTurn = NextPlayer;

        //Update cards only for person that had turn now
        await Clients.Client(player.ConnectionID).SendAsync("SetPlayerTurn", JsonSerializer.Serialize(new UnoPlayerTurnInfo(player)));

        //Update turn for all players
        await UpdateRound(uno);
        foreach(UnoGamePlayer Player in uno.Players)
        {
           
            /*
            if(Player.ConnectionID == uno.PlayerTurn.ConnectionID)
            {
                Console.WriteLine("Changing SetPlayerTurn");
                Clients.Client(Player.ConnectionID).SendAsync("SetPlayerTurn", JsonSerializer.Serialize(new UnoPlayerTurnInfo(Player)));
            }
            else
            {
                Clients.Client(Player.ConnectionID).SendAsync("SetSpecTurn", JsonSerializer.Serialize(new UnoPlayerTurnInfo(Player)));
            }
            */
        }
    }

    public void PlayerAddCard(int playerID)
    {
        uno.Players[playerID].AddCard();
    }

    public async Task StartGame()
    {
        uno.GameStart();

        foreach (var player in uno.Players)
        {
            player.Cards.Clear();
            for (int i = 0; i < UnoRuleset.START_CARD_COUNT; i++)
            {
                player.AddCard();
            }
            
            await Clients.Client(player.ConnectionID).SendAsync("StartGame", JsonSerializer.Serialize(new UnoGameStartInfo(uno,player)));
        }
    }

    public async Task UpdateRound(UnoGameLobby lobby)
    {
        await Clients.All.SendAsync("UpdateRound", JsonSerializer.Serialize(new UnoRoundInfo(lobby)));
    }
}

public class UnoRuleset
{
    public const int START_CARD_COUNT = 7;

    public static bool IsValidCard(UnoGameCard card, UnoGameCard cardOnTop)
    {
        if (cardOnTop.Color == card.Color)
        {
            return true;
        }
        if (cardOnTop.Power == card.Power)
        {
            return true;
        }
        return false;
    }

    public static void RegisterActionCard(IHubCallerClients hub,UnoGameCard card, UnoGameLobby lobby)
    {
        switch (card.Power)
        {
            case 11:
                lobby.GetNextPlayer(lobby.PlayerTurn.ConnectionID).AddCards(2);
                break;
        }
    }
}

public class UnoGameLobby
{
    public List<UnoGamePlayer> Players = new();

    public UnoDeck Deck;
    public UnoGamePlayer PlayerTurn;

    public bool isReversedRound = false;

    public UnoGameCard CardOnTop { get; set;}

    public int PlayersCount => Players.Count;

    public void GameStart()
    {
        Deck = new UnoDeck();
        CardOnTop = Deck.GetRandomCard();
        SetRandomTurn();
    }

    public UnoGamePlayer GetNextPlayer(string ConnectionID)
    {
        int index = Players.FindIndex(x => x.ConnectionID == ConnectionID);

        UnoGamePlayer NextPlayer;
        if(!isReversedRound)
        {
            if(index+1 >= PlayersCount)
            {
                NextPlayer = Players.ElementAt(0);
            }
            else
            {
                NextPlayer = Players.ElementAt(index + 1);
            }
        }
        else
        {
            if(index -1 == 0)
            {
                NextPlayer = Players.ElementAt(Players.Count);
            }
            else
            {
                NextPlayer = Players.ElementAt(index - 1);
            }
        }

        return NextPlayer;
    }

    public void RemovePlayer(UnoGamePlayer? player)
    {
        if(player != null)
            Players.Remove(player);
    }

    public void SetRandomTurn()
    {
        PlayerTurn = Players[Random.Shared.Next(Players.Count)];
    }

    public void SetCardOnTop(UnoGameCard card)
    {
        CardOnTop = card;
    }

    public void AddPlayer(string ConnectionID, string username,UnoGameLobby game)
    {
        bool _Created = false;

        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i] is null)
            {
                _Created = true;
                Players.Insert(i, new UnoGamePlayer(username, ConnectionID,this));
                break;
            }
        }

        if (!_Created)
        {
            Players.Add(new UnoGamePlayer(username, ConnectionID,game));
        }
    }

    public void ClearLobby()
    {
        Players.Clear();
    }

    public string GetInfo()
    {
        return JsonSerializer.Serialize(new UnoGameLobbyInfo(Players));
    }
}

public class UnoGamePlayer
{
    public List<UnoGameCard> Cards = new();
    public UnoGameLobby Game;
    public string PlayerName { get; set; }

    public string ConnectionID { get; set; }

    public void AddCard()
    {
        UnoGameCard card;
        if((card = Game.Deck.GetRandomCard()) != null)
        {
            Cards.Add(card);
        }    
    }

    public void AddCards(int count)
    {
        for(int i = 0; i < count; i++)
        {
            AddCard();
        }
    }

    public void RemoveCard(UnoGameCard card)
    {
        UnoGameCard? c = Cards.Find(x=> x.Color == card.Color && x.Power == card.Power);
        if(c != null)
        {
            Cards.Remove(c);    
        }
    }

    public UnoGamePlayer(string playerName, string connectionID,UnoGameLobby game)
    {
        PlayerName = playerName;
        ConnectionID = connectionID;
        Game = game;
    }
}

public class UnoDeck
{
    private Stack<UnoGameCard> Deck = new();
    private static readonly string[] Colors = new string[] { "blue", "red", "green", "yellow" };
    private static readonly string[] Actions = new string[] { "reverse", "plus2"};


    public UnoGameCard? GetRandomCard()
    {
        UnoGameCard? _;
        if(Deck.TryPop(out _))
        {
            return _;
        }
        else
        {
            Console.WriteLine("NO_CARDS_AVAIBLE: count:=" +Deck.Count);
            return _;
        }
       
    }

    public UnoDeck()
    {
        List<UnoGameCard> _ = new(108);
        int powerStart = 0;

        for(int i=0; i<2;i++)
        {
            for(int color =0;color < Colors.Length;color ++)
            {
                for(int power = powerStart; power<10;power++)
                {
                    _.Add(new UnoGameCard(Colors[color], power));
                }
            }
            powerStart = 1;
        }

        for(int i =0;i<2;i++)
        {
            for(int color =0;color<Colors.Length;color++)
            {
                for(int action = 0; action<Actions.Length;action++)
                {
                    _.Add(new UnoGameCard(Colors[color],10+action));
                }
            }
        }

        //SUFFLE CARDS
        int n = _.Count;  
        while(n > 1)
        {
            n--;
            int k = Random.Shared.Next(n + 1);
            UnoGameCard value = _[k];
            _[k] = _[n];
            _[n] = value;
        }
        
        Deck = new Stack<UnoGameCard>(_);
    }
}

[Serializable]
public class UnoGameStartInfo
{
    public string[] Players { get; init; }
    public string PlayerTurn { get; init; }

    public UnoGameCard CardOnTop { get; init; }
    public UnoGameCard[] Cards { get; init; }

    public UnoGameStartInfo(UnoGameLobby game, UnoGamePlayer player)
    {
        Players = game.Players.Select(x => x.PlayerName).ToArray();
        PlayerTurn = game.PlayerTurn.PlayerName;
        CardOnTop = game.CardOnTop;
        Cards = player.Cards.ToArray();
    }
}

///Packet avaible for all clients
[Serializable]
public class UnoRoundInfo
{
    public UnoGameCard CardOnTop { get; init; }
    public string PlayerTurn { get; init; }
    
    public UnoRoundInfo(UnoGameLobby game)
    {
        CardOnTop = game.CardOnTop;
        PlayerTurn = game.PlayerTurn.PlayerName;
    }
}

//Packet avaible for only one client
[Serializable]
public class UnoPlayerTurnInfo
{
    public UnoGameCard[] Cards { get; init; }
    
    public UnoPlayerTurnInfo(UnoGamePlayer player)
    {
        Cards = player.Cards.ToArray();
    }
}

[Serializable]
public class UnoSpecInfo
{
    public int CardsCount { get; init; }
    
    public UnoSpecInfo(UnoGamePlayer player)
    {
        CardsCount = player.Cards.Count;
    }
}

///Packet avaible for all
[Serializable]
public class UnoGameLobbyInfo
{
    public UnoGamePlayer[] Players { get; init; }
    public bool GameStarted { get; set;} = false;

    public UnoGameLobbyInfo(IEnumerable<UnoGamePlayer> players)
    {
        Players = players.ToArray();
    }
}

///Packet avaible for all
[Serializable]
public class UnoGameCard
{
    public string Color { get; init;}
    public int Power { get; init;}

    public UnoGameCard(string color, int power)
    {
        Color = color;
        Power = power;  
    }
}

public class UnoRulesetInfo
{
    public string ActionName { get; init; }
}