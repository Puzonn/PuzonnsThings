import { Options } from "./YahtzeeTypes";

export interface LobbyUser {
  Username: string;
  LobbyPlace: number | undefined;
  UserId: number;
}

export interface ILobbyOptions {
  LobbyOptions: Options;
  IsCreator: boolean;
  LobbyId: number;
  LobbyUsers: LobbyUser[];
  MaxPlayers: number;
  MinPlayers: number;
  StartReadyState: boolean;
  OnChangeMaxPlayersState: (state: number) => void;
  OnChoosePlaceState: (placeId: number) => void;
  OnLobbyPlaceClick: (placeId: number) => void;
  OnStartClick: () => void;
}