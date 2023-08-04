import axios from "axios";
import "./Lobbies.css";
import { useContext, useEffect, useState } from "react";
import { Config } from "../Shared/Config";
import { Lobby } from "../Types/LobbiesType";
import { useSearchParams } from "react-router-dom";
import { Auth } from "../Auth/Auth";
import { AuthContext } from "../Shared/AuthContext";

export const Lobbies = () => {
  const [Lobbies, SetLobbies] = useState<Lobby[]>([]);
  const [params] = useSearchParams();
  const { isLoggedIn } = useContext(AuthContext);
  const [fetchError, setFetchError] = useState("");
  const [queryTypeSelector, setQueryTypeSelector] = useState("");

  const FetchLobbies = (queryType: string) => {
    axios
      .get(Config.GetApiUrl() + `/api/lobbies/fetch/${queryType}`)
      .then((response) => {
        if (response.status !== 200) {
          setFetchError(`Internal error with ${response.status} status code`);
          return;
        }

        if (response.data.length === 0) {
          setFetchError(`We couldn't find any lobbies`);
          return;
        }

        const lobbies: Lobby[] = [];
        for (const lobby of response.data) {
          lobbies.push({
            LobbyCreator: lobby.lobbyCreator,
            PlayerCount: lobby.playersCount,
            MaxPlayerCount: lobby.maxPlayersCount,
            LobbyId: lobby.id,
            LobbyStatus: lobby.status,
            LobbyType: lobby.lobbyType,
          });
        }
        setFetchError("");
        SetLobbies(lobbies);
      });
  };

  useEffect(() => {
    const queryType = params.get("type");

    if (typeof queryType !== "undefined" && queryType !== null) {
      FetchLobbies(queryType);

      setQueryTypeSelector(queryType);
      return;
    }

    axios.get(Config.GetApiUrl() + "/api/lobbies/fetch").then((response) => {
      if (response.status !== 200) {
        setFetchError(`Internal error with ${response.status} status code`);
        return;
      }

      if (response.data.length === 0) {
        setFetchError(`We couldn't find any lobbies`);
        return;
      }

      const lobbies: Lobby[] = [];

      for (const lobby of response.data) {
        lobbies.push({
          LobbyCreator: lobby.lobbyCreator,
          PlayerCount: lobby.playersCount,
          MaxPlayerCount: lobby.maxPlayersCount,
          LobbyId: lobby.id,
          LobbyStatus: lobby.status,
          LobbyType: lobby.lobbyType,
        });
      }
      setFetchError("");
      SetLobbies(lobbies);
    });
  }, []);

  const CreateLobbyHandler = (event: any) => {
    event.preventDefault();
    const type = event.target["type"].value;

    axios
      .post(
        Config.GetApiUrl() + `/api/lobbies/create/${type}`,
        {},
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: Auth.GetAuthorizationHeader(),
          },
        }
      )
      .then((response) => {
        if (response.status === 200) {
          window.location.href = `/yahtzee?id=${response.data.id}`;
        }
      });
  };

  const JoinLobby = (lobbyType: string, lobbyId: number) => {
    if (lobbyType === "Yahtzee") {
      window.location.href = `/yahtzee?id=${lobbyId}`;
    }
  };

  const OnTypeChange = (event: any) => {
    const queryType = event.target.value;
    setQueryTypeSelector(queryType);
    window.history.pushState("", "", `/lobbies?type=${queryType}`);

    FetchLobbies(queryType);
  };

  return (
    <div id="lobbies">
      <div id="lobbies-selector">
        <form onSubmit={CreateLobbyHandler}>
          <button name="submit" type="submit" id="lobbies-create_lobby-btn">
            Create Lobby
          </button>
          <select
            onChange={OnTypeChange}
            value={queryTypeSelector}
            name="type"
            title="Lobby type selector"
          >
            <option value="yahtzee">Yahtzee</option>
            <option value="watchtogether">Watch Together</option>
          </select>
        </form>
      </div>
      {fetchError === "" && (
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Creator</th>
              <th>Type</th>
              <th>Players</th>
            </tr>
          </thead>
          <tbody>
            {Lobbies.map((x, index) => {
              return (
                <tr key={`lobby_${index}`}>
                  <td>{x.LobbyId}</td>
                  <td>{x.LobbyCreator}</td>
                  <td>{x.LobbyType}</td>
                  <td>
                    {x.PlayerCount}/{x.MaxPlayerCount}
                  </td>
                  <td>
                    {isLoggedIn && (
                      <button onClick={() => JoinLobby(x.LobbyType, x.LobbyId)}>
                        Join
                      </button>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
      {fetchError != "" && (
        <div style={{ textAlign: "center" }}>
          <h2>{fetchError}</h2>
          <span style={{ color: "var(--color-grey)" }}>
            You can create lobby by clicking
            <span style={{ color: "var(--color-green)" }}> green button </span>!
          </span>
        </div>
      )}
    </div>
  );
};