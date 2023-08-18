import { ILobbyOptions, LobbyUser } from "../Types/LobbyManagerTypes";
import "./LobbyManager.css";
import { useContext, useEffect, useState } from "react";
import KickIcon from "../Icons/icon_close.svg";
import PlayersIcon from "../Icons/icon_players.png";
import PrivacyIcon from "../Icons/icon_privacy.png";
import ClockIcon from "../Icons/icon_clock.png";
import { UserContext } from "./UserContext";

export const LobbyManager = ({
  LobbyId,
  IsCreator,
  LobbyUsers,
  OnChangeMaxPlayersState,
  OnChangePrivacyState,
  OnChoosePlaceState,
  OnLobbyPlaceClick,
  MaxPlayers,
  StartReadyState,
  LobbyOptions,
  OnChangeGameTimeState,
  OnStartClick,
}: ILobbyOptions) => {
  const { UserId } = useContext(UserContext);
  const [lobbyUsers, setLobbyUsers] = useState<LobbyUser[]>([]);
  const [navSelected, setNavSelected] = useState(0);

  const ShouldShowPlayers = navSelected === 0;
  const ShouldShowOptions = navSelected === 1;
  const ShouldShowStartButton = IsCreator && StartReadyState;

  useEffect(() => {
    setLobbyUsers(LobbyUsers);
    console.log(LobbyUsers);
  });

  const ChoosePlaceState = (place: number) => {
    const hasPlace = lobbyUsers.find((x) => x.UserId === UserId);
    if (hasPlace?.LobbyPlace === -1) {
      OnChoosePlaceState(place);
    }
  };

  return (
    <div className="mg_lobby_place_container">
      <div className="mg_lobby_places">
        {[...Array(MaxPlayers).keys()].map((val, index) => {
          const lobbyUser = lobbyUsers.find((x) => x.LobbyPlace === index);
          const shouldRenderKick = IsCreator || lobbyUser?.UserId === UserId;
          return (
            <div
              onClick={() => ChoosePlaceState(index)}
              className="mg_lobby_place"
              key={`mg_lbplace_${index}`}
            >
              {lobbyUser && (
                <div
                  style={{
                    position: "relative",
                    border: `1px solid ${lobbyUser.Avatar}`,
                    borderRadius: "10px",
                  }}
                >
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
                        alt="icon_lobby_kick_icon"
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
                <div
                  style={{
                    border: "1px solid #6A6D81",
                    borderRadius: "10px",
                  }}
                >
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
            className={
              "mg_nav-left " +
              (navSelected == 0 ? "mg_nav-selected" : "mg_nav-unselected")
            }
          >
            <strong>Players</strong>
          </li>
          <li
            onClick={() => setNavSelected(1)}
            className={
              "mg_nav-right " +
              (navSelected == 0 ? "mg_nav-unselected" : "mg_nav-selected")
            }
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
                      <span style={{ color: value.Avatar }}>
                        {value.Username}{" "}
                      </span>
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
              <div>
                <div className="mg_lobby_options-name-container">
                  <img src={PlayersIcon} />
                  <span>Max Players</span>
                </div>
                <div className="mg_lobby_options_container">
                  <button
                    onClick={() => OnChangeMaxPlayersState(2)}
                    className={
                      "mg_lobby_options-option " +
                      (MaxPlayers == 2
                        ? "mg_lobby_options-options-selected"
                        : "")
                    }
                  >
                    2 Players
                  </button>
                  <button
                    onClick={() => OnChangeMaxPlayersState(3)}
                    style={{ marginLeft: "10px" }}
                    className={
                      "mg_lobby_options-option " +
                      (MaxPlayers == 3
                        ? "mg_lobby_options-options-selected"
                        : "")
                    }
                  >
                    3 Players
                  </button>
                  <button
                    onClick={() => OnChangeMaxPlayersState(4)}
                    style={{ marginLeft: "10px" }}
                    className={
                      "mg_lobby_options-option " +
                      (MaxPlayers == 4
                        ? "mg_lobby_options-options-selected"
                        : "")
                    }
                  >
                    4 Players
                  </button>
                </div>
              </div>
              <div>
                <div className="mg_lobby_options-name-container">
                  <img alt="lobby_privacy_icon" src={PrivacyIcon} />
                  <span>Privacy</span>
                </div>
                <div className="mg_lobby_options_container">
                  <button
                    onClick={() => OnChangePrivacyState(true)}
                    className={
                      "mg_lobby_options-option " +
                      (LobbyOptions.isPublic
                        ? "mg_lobby_options-options-selected"
                        : "")
                    }
                  >
                    Public
                  </button>
                  <button
                    onClick={() => OnChangePrivacyState(false)}
                    style={{ marginLeft: "10px" }}
                    className={
                      "mg_lobby_options-option " +
                      (!LobbyOptions.isPublic
                        ? "mg_lobby_options-options-selected"
                        : "")
                    }
                  >
                    Private
                  </button>
                </div>
              </div>
              <div>
                <div className="mg_lobby_options-name-container">
                  <img alt="lobby_clock_icon" src={ClockIcon} />
                  <span>Game Time</span>
                </div>
                <div className="mg_lobby_options_container">
                  <button
                    onClick={() => OnChangeGameTimeState(60)}
                    className={
                      "mg_lobby_options-option " +
                      (LobbyOptions.gameTime === 60
                        ? "mg_lobby_options-options-selected"
                        : "")
                    }
                  >
                    1 Minute
                  </button>
                  <button
                    onClick={() => OnChangeGameTimeState(120)}
                    style={{ marginLeft: "10px" }}
                    className={
                      "mg_lobby_options-option " +
                      (LobbyOptions.gameTime === 120
                        ? "mg_lobby_options-options-selected"
                        : "")
                    }
                  >
                    2 Minutes
                  </button>
                  <button
                    onClick={() => OnChangeGameTimeState(240)}
                    style={{ marginLeft: "10px" }}
                    className={
                      "mg_lobby_options-option " +
                      (LobbyOptions.gameTime === 240
                        ? "mg_lobby_options-options-selected"
                        : "")
                    }
                  >
                    4 Minutes
                  </button>
                </div>
              </div>
              <button className="mg_lobby_start-btn">Start</button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
