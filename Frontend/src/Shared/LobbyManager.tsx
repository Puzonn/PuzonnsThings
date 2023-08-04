import { ILobbyOptions, LobbyUser } from "../Types/LobbyManagerTypes";
import "./LobbyManager.css";
import { useEffect, useState, useContext } from "react";
import KickIcon from "../Icons/icon_close.svg";
import { UserContext } from "./UserContext";

export const LobbyManager = ({
  LobbyId,
  IsCreator,
  LobbyUsers,
  OnChangeMaxPlayersState,
  OnChoosePlaceState,
  OnLobbyPlaceClick,
  MaxPlayers,
  StartReadyState,
  LobbyOptions,
  OnStartClick,
  MinPlayers,
}: ILobbyOptions) => {
  const { UserId } = useContext(UserContext);
  const [maxPlayers, setMaxPlayers] = useState(MinPlayers);
  const [lobbyUsers, setLobbyUsers] = useState<LobbyUser[]>([]);
  const [navSelected, setNavSelected] = useState(0);

  const ShouldShowPlayers = navSelected === 0;
  const ShouldShowOptions = navSelected === 1;
  const ShouldShowStartButton = IsCreator && StartReadyState;
  
  useEffect(() => {
    setLobbyUsers(LobbyUsers);

    if (LobbyOptions.maxPlayersCount !== -1) {
      setMaxPlayers(LobbyOptions.maxPlayersCount);
    }
  });

  const OptionsChangeMaxPlayers = (count: number) => {
    OnChangeMaxPlayersState(count);
  };

  const ChoosePlaceState = (place: number) => {
    const hasPlace = lobbyUsers.find((x) => x.UserId === UserId);
    if (hasPlace?.LobbyPlace === -1) {
      OnChoosePlaceState(place);
    }
  };

  return (
    <div style={{ maxWidth: "400px" }}>
      <div className="mg_lobby_place_container">
        <div>
          {[...Array(maxPlayers)].map((val, index) => {
            const lobbyUser = lobbyUsers.find((x) => x.LobbyPlace === index);
            const shouldRenderKick = IsCreator || lobbyUser?.UserId === UserId;
            return (
              <div
                onClick={() => ChoosePlaceState(index)}
                className="mg_lobby_place"
                key={`mg_lbplace_${index}`}
              >
                {lobbyUser && (
                  <div style={{ position: "relative" }}>
                    <span>#{index + 1}</span>
                    <p style={{ fontSize: "15px", color: "var(--text-color)" }}>
                      <strong> {lobbyUser.Username}</strong>
                    </p>
                    {shouldRenderKick && (
                      <div
                        style={{
                          width: "25px",
                          position: "absolute",
                          top: "12px",
                          left: "86%",
                          height: "25px",
                          borderRadius: "5px",
                          backgroundColor: "#ffffff08",
                        }}
                      >
                        <img
                          onClick={() => {
                            OnLobbyPlaceClick(index);
                          }}
                          style={{ width: "25px" }}
                          src={KickIcon}
                        />
                      </div>
                    )}
                  </div>
                )}
                {!lobbyUser && (
                  <div>
                    <span>#{index + 1}</span>
                    <p>Choose a place</p>
                  </div>
                )}
              </div>
            );
          })}
        </div>
        <div className="mg_lobby_adm">
          <ul className="mg_lobby_nav">
            <li
              onClick={() => setNavSelected(0)}
              className={navSelected == 0 ? "mg_nav-selected" : ""}
            >
              <strong>Players</strong>
            </li>
            <li
              onClick={() => setNavSelected(1)}
              className={navSelected == 1 ? "mg_nav-selected" : ""}
            >
              <strong>Options</strong>
            </li>
          </ul>
          <div>
            <br />
            {ShouldShowPlayers && (
              <div className="mg_lobby_players">
                {lobbyUsers.map((value, index) => {
                  return (
                    <div key={`player_name_${value.Username}`}>
                      <p style={{ position: "relative" }}>
                        <span>{value.Username} </span>
                        {value.LobbyPlace !== -1 && (
                          <span style={{ color: "var(--color-grey)" }}>
                            #{value.LobbyPlace}
                          </span>
                        )}
                      </p>
                    </div>
                  );
                })}
              </div>
            )}
            {ShouldShowOptions && (
              <div className="mg_lobby_options">
                <p style={{ fontSize: "12px", color: "var(--color-grey)" }}>
                  Any changes will restart the lobby
                </p>
                <span>Max Players</span>
                <select
                  className="mg_lobby_players-input"
                  value={maxPlayers}
                  onChange={(event) => {
                    OptionsChangeMaxPlayers(parseInt(event.target.value));
                  }}
                >
                  {[...Array(MaxPlayers - 1)].map((val, index) => {
                    return (
                      <option
                        value={index == 0 ? MinPlayers : index + MinPlayers}
                        key={`mg_max_players_${index}`}
                      >
                        {index == 0 ? MinPlayers : index + MinPlayers}
                      </option>
                    );
                  })}
                </select>
                <span>Game Time</span>
                <select className="mg_lobby_players-input">
                  {[...Array(MaxPlayers)].map((val, index) => {
                    return (
                      <option key={`mg_game_time${index}`}>
                        {index == 0 ? 1 : index * 3} min
                      </option>
                    );
                  })}
                </select>
              </div>
            )}
          </div>
        </div>
      </div>
      <div style={{ textAlign: "center" }}>
        {ShouldShowStartButton && <button onClick={OnStartClick}>Start</button>}
      </div>
    </div>
  );
};
