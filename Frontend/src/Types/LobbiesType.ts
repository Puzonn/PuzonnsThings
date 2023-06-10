export interface Lobby
{
    LobbyCreator: string;
    PlayerCount: number;
    MaxPlayerCount: number;
    LobbyType: string;
    LobbyId: number;
    LobbyStatus: LobbyStatus;
}

enum LobbyStatus 
{
    Started = 0,
    Waiting = 1
}