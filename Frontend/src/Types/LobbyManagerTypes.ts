import { Options } from "./YahtzeeTypes";

export interface LobbyUser {
  Username: string;
  Avatar: string;
  LobbyPlace: number | undefined;
  UserId: number;
}

export interface ILobbyOptions {
  PrivacyOption: boolean;
  LobbyOptions: Options;
  IsCreator: boolean;
  LobbyId: number;
  LobbyUsers: LobbyUser[];
  MaxPlayers: number;
  MinPlayers: number;
  StartReadyState: boolean;
  OnChangeMaxPlayersState: (state: number) => void;
  OnChangePrivacyState: (state: boolean) => void;
  OnChoosePlaceState: (placeId: number) => void;
  OnLobbyPlaceClick: (placeId: number) => void;
  OnChangeGameTimeState: (state: number) => void;
  OnStartClick: () => void;
}
