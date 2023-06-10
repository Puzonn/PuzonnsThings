import axios from "axios";
import "./Lobbies.css";
import { useEffect, useState } from "react";
import { Config } from "../Shared/Config";
import { Lobby } from "../Types/LobbiesType";
import { useSearchParams } from "react-router-dom";
import { Auth } from "../Auth/Auth";

export const Lobbies = () => {
  const [Lobbies, SetLobbies] = useState<Lobby[]>([]);
  const [params] = useSearchParams();

  useEffect(() => {
    const queryGame = params.get("game");
    if (typeof queryGame !== "undefined" && queryGame !== null) {
      axios
        .get(Config.GetApiUrl() + `/api/lobbies/fetch/${queryGame}`)
        .then((response) => {
          if (response.status !== 200) {
            return;
          }

          const lobbies: Lobby[] = [];
          for (let i = 0; i < 10; i++) {
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
          }
          SetLobbies(lobbies);
        });
    } else {
      axios.get(Config.GetApiUrl() + "/api/lobbies/fetch").then((response) => {
        if (response.status !== 200) {
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
        SetLobbies(lobbies);
      });
    }
  }, []);

  const CreateLobbyHandler = (event: any) => {
    event.preventDefault();
    const type = event.target["type"].value;

    const test = 
    {
      'uwu': 'aa'
    }

    axios.post(Config.GetApiUrl() + `/api/lobbies/create/${type}`, {}, {
      headers: {
        "Content-Type": "application/json",
        Authorization: Auth.GetAuthorizationHeader(),
      },
    }).then(response => {
      if(response.status === 200){
        window.location.href = `/yahtzee?id=${response.data.id}`
      }
    })
  };

  const JoinLobby = (lobbyType: string, lobbyId: number) => {
    if(lobbyType === "Yahtzee"){
      window.location.href =`/yahtzee?id=${lobbyId}`
    }
  }
  
  return (
    <div id="lobbies">
      <div id="lobbies-selector">
        <form onSubmit={CreateLobbyHandler}>
          <button name="submit" type="submit" id="lobbies-create_lobby-btn">
            Create Lobby
          </button>
          <select name="type" title="Lobby type selector">
            <option value="yahtzee">Yahtzee</option>
            <option value="watchtogether">Watch Together</option>
          </select>
        </form>
      </div>
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
                  <button onClick={() => JoinLobby(x.LobbyType, x.LobbyId)}>Join</button>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};
