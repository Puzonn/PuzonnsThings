export interface Room 
{
    VideoTitle: string;
    VideoId: string;
    RoomCreator: string;
    RoomWatchers: number;
    RoomId: number;
}

export interface RoomSync 
{
    CurrentTime: number;
    IsPaused: boolean;
}
