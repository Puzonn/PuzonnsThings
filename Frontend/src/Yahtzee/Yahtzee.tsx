import { useContext, useEffect, useState } from "react";
import "./Yahtzee.css";
import {
  Dice,
  PointType,
  RoomModel,
  Player,
  PlayerCell,
  PointCell,
  Endgame,
  PointCells,
  Options,
  OptionsMaxPlayersState,
  GameTimer,
  Lobby,
} from "../Types/YahtzeeTypes";
import * as signalR from "@microsoft/signalr";
import { Config } from "../Shared/Config";
import { Auth } from "../Auth/Auth";
import { useSearchParams } from "react-router-dom";
import { UserContext } from "../Shared/UserContext";
import { LobbyManager } from "../Shared/LobbyManager";
import { CalculatePoints } from "./YahtzeeCalculator";
import {
  GetCellClass,
  GetDiceImage,
  GetPointsFromType,
  HasSetPoint,
} from "./YahtzeeUtil";

const hubConnection = new signalR.HubConnectionBuilder()
  .withUrl(Config.GetApiUrl() + "/services/yahtzeeservice", {
    accessTokenFactory: () => Auth.GetAuthorizationToken(),
  })
  .build();

export const Yahtzee = () => {
  const user = useContext(UserContext);

  const [dices, setDices] = useState<Dice[]>([]);
  const [isRolling, setRollState] = useState<boolean>(false);
  const [selectedDices, setSelectedDices] = useState<number[]>([]);
  const [hasRound, setHasRound] = useState<boolean>(false);
  const [isCreator, setCreator] = useState<boolean>(false);
  const [lobby, setLobby] = useState<Lobby>({
    playerCells: [],
    lobbyId: -1,
    options: {
      gameTime: -1,
      isPublic: false,
      maxPlayers: 2,
    },
    gameStarted: false,
    players: [],
    startState: false,
    isCreator: false,
  });
  const [rollCount, setRollCount] = useState<number>(2);
  const [gameEnded, setGameEnded] = useState<boolean>(false);
  const [endgameScreen, setEndgameScreen] = useState<Endgame>();
  const [timerTime, setTimerTime] = useState(0);
  const [playerTimes, setPlayerTimes] = useState({});
  const [timerState, setTimerState] = useState(false);

  const [params] = useSearchParams();
  const cells = PointCells;

  const ChangeMaxPlayersState = (state: number) => {
    hubConnection.send("OnOptionsMaxPlayersChange", state);
  };

  const ChangeMaxPlayersStateCallback = (state: OptionsMaxPlayersState) => {
    setLobby((prev) => {
      const option = prev.options;
      option.maxPlayers = state.maxPlayersState;
      return { ...prev, options: option, players: state.players };
    });
  };

  const ChoosePlaceStateCallback = (response: any) => {
    setLobby((prev) => {
      return {
        ...prev,
        players: response.players,
        startState: response.startState,
      };
    });
  };

  useEffect(() => {
    setTimeout(() => {
      //Just a reducer to update times
      setTimerTime((prev) => {
        return prev + 1;
      });

      setPlayerTimes((prev) => {
        if (!lobby.gameStarted || timerState === false) {
          return prev;
        }

        const playerRound = lobby.players.find((x) => x.hasRound);

        if (!playerRound) {
          console.error("Cannot find playerRound");
          return prev;
        }

        (prev as any)[playerRound.userId] = playerRound.gameTime -= 1;
        return prev;
      });
    }, 1000);
  }, [timerTime]);

  const LobbyPlaceClick = (placeId: number) => {
    hubConnection.send("OnLobbyPlaceClick", placeId);
  };

  //Changes privacy option, where true == private game
  const PrivacyChange = (state: boolean) => {};

  const ChoosePlaceState = (placeId: number) => {
    hubConnection.send("OnChoosePlaceState", placeId);
  };

  const SetPointsFromType = (
    userId: number,
    pointType: PointType,
    points: number
  ) => {
    const placedPoints = [...lobby.playerCells];

    const point = placedPoints.find((x) => x.userId === userId);

    if (typeof point === "undefined") {
      console.error(`Point of ${userId} has undefined row ${pointType}`);
      return;
    }
    point.pointCell[pointType] = { pointType: pointType, points: points };

    setLobby((prev) => {
      return { ...prev, playerCells: placedPoints };
    });
  };

  useEffect(() => {
    const idParm = params.get("id");

    if (idParm !== null) {
      JoinRoom(parseInt(idParm));

      return;
    }

    Auth.IsLoggedIn((success) => {
      if (!success) {
        window.location.href = "/login";
      }
    });
  }, []);

  const OnEndGame = (data: any) => {
    console.log(data);
    const endgame: Endgame = data;

    setEndgameScreen(endgame);
    setGameEnded(true);
    setTimerState(false);
    user.fetchUpdated();
  };

  const OnJoin = (data: any) => {
    const cells: PlayerCell[] = [];

    for (const player of data.players) {
      const placedPoints: PointCell[] = [];
      for (const placedPoints of player.placedPoints) {
        placedPoints.push({
          pointType: placedPoints.point,
          points: placedPoints.pointsFromPoint,
        });
      }
      cells.push({ userId: player.userId, pointCell: placedPoints });
    }

    const joinnedLobby: Lobby = {
      players: data.players,
      options: data.options,
      playerCells: cells,
      gameStarted: data.gameStarted,
      lobbyId: data.lobbyId,
      startState: data.startState,
      isCreator: data.isCreator,
    };
    console.log(data.options)
    const times = {};
    for (const player of data.players) {
      (times as any)[player.userId] = player.gameTime;
    }

    setPlayerTimes(times);
    setLobby(joinnedLobby);

    setRollCount(data.rollCount);
    setHasRound(data.hasRound);
    setCreator(data.isCreator);
    setDices(data.dices);
    setTimerState(joinnedLobby.gameStarted);
  };

  useEffect(() => {
    if (gameEnded) {
      return;
    }

    if (rollCount > 0 && lobby.gameStarted) {
      setRollCount(rollCount - 1);
      setSelectedDices([]);
      hubConnection.send("OnRollDices", selectedDices);
    }
  }, [isRolling]);

  const GetCell = (userId: number, pointType: PointType) => {
    const playerRound = lobby.players.find((x) => x.hasRound);

    if (!playerRound) {
      console.error(`Can't calculate cell <${userId}>`);
      return "<undefined>";
    }

    if (playerRound.userId !== userId) {
      return GetPointsFromType(lobby.playerCells, userId, pointType);
    }

    return CalculatePoints(dices, pointType);
  };

  useEffect(() => {
    if (!lobby.gameStarted) {
      return;
    }
    for (let i = 0; i < dices.length; i++) {
      if (selectedDices.includes(dices[i].index)) {
        const diceElement = document.getElementById(`dice_${i}`);

        if (diceElement === null) {
          console.error("dice Element is null");
          continue;
        }

        diceElement.style.border = "1px solid red";
      } else {
        const diceElement = document.getElementById(`dice_${i}`);

        if (diceElement === null) {
          console.error("dice Element is null");
          continue;
        }

        diceElement.style.border = "";
      }
    }
  }, [selectedDices]);

  const OnNextRound = (data: any) => {
    const cells: PlayerCell[] = [];

    setRollCount(2);
    setSelectedDices([]);
    setDices(data.dices);

    for (const player of data.players) {
      const placedPoints: PointCell[] = [];

      for (const placedPoints of player.placedPoints) {
        placedPoints.push({
          pointType: placedPoints.point,
          points: placedPoints.pointsFromPoint,
        });
      }
      cells.push({ userId: player.userId, pointCell: placedPoints });
    }
    setHasRound(data.hasRound);
    setLobby((prev) => {
      return { ...prev, playerCells: cells, players: data.players };
    });
  };

  const OnPointSet = (pointType: PointType) => {
    if (gameEnded) {
      return;
    }
    hubConnection.invoke("OnPointSet", pointType).then((x) => {
      if (!x.isSuccessFul) {
        return;
      }

      SetPointsFromType(user.UserId, x.point, x.pointsFromPoint);
    });
  };

  const SelectToRollDice = (dice: Dice) => {
    if (!hasRound) {
      return;
    }

    const element = document.getElementById(`dice_${dice.index}`);

    if (element === null) return;

    if (dice.isSelected) {
      dice.isSelected = false;
      element.style.border = "";

      const mSelectedDices = [...selectedDices];
      const diceArrayIndex = mSelectedDices.indexOf(dice.index);
      mSelectedDices.splice(diceArrayIndex, 1);
      setSelectedDices(mSelectedDices);
    } else {
      dice.isSelected = true;
      element.style.border = "1px solid red";

      const mSelectedDices = [...selectedDices, dice.index];

      setSelectedDices(mSelectedDices);
    }
  };

  const JoinRoom = (roomId: number) => {
    hubConnection.start().then(() => {
      window.history.pushState("", "", "/yahtzee?id=" + roomId);

      hubConnection.on("SetDices", setDices);
      hubConnection.on(
        "ChangeMaxPlayersStateCallback",
        ChangeMaxPlayersStateCallback
      );
      hubConnection.on("OnJoin", OnJoin);
      hubConnection.on("OnNextRound", OnNextRound);
      hubConnection.on("OnEndGame", OnEndGame);
      hubConnection.on("ForcedLeave", OnForcedLeave);
      hubConnection.on("ChoosePlaceStateCallback", ChoosePlaceStateCallback);
      hubConnection.invoke("Join", roomId).then((canJoin) => {});
    });
  };

  const OnForcedLeave = (reason: string) => {
    hubConnection.stop();
    console.warn(reason);
    window.location.href = "/lobbies?type=yahtzee";
  };

  const StartGame = () => {
    hubConnection.invoke("StartGame");
  };

  if (!lobby.gameStarted) {
    return (
      <div>
        <LobbyManager
          MaxPlayers={lobby.options.maxPlayers}
          OnStartClick={StartGame}
          StartReadyState={lobby.startState}
          LobbyOptions={lobby.options}
          OnChoosePlaceState={ChoosePlaceState}
          OnChangeMaxPlayersState={ChangeMaxPlayersState}
          OnLobbyPlaceClick={LobbyPlaceClick}
          LobbyId={lobby.lobbyId}
          PrivacyOption={true}
          MinPlayers={2}
          LobbyUsers={lobby.players.map(
            ({ username, lobbyPlaceId, userId }) => ({
              Username: username,
              LobbyPlace: lobbyPlaceId,
              UserId: userId,
            })
          )}
          IsCreator={isCreator}
        ></LobbyManager>
      </div>
    );
  }

  return (
    <div>
      <table className="yahtzee-container">
        <tbody>
          <tr id="yahtzee-player_container">
            <th></th>
            {lobby.players.map((player, index) => {
              return (
                <th
                  key={`yahtzee_player_cell_${player.username}`}
                  style={{ width: "150px" }}
                >
                  <div
                    style={{
                      color: player.hasRound ? "white" : "var(--color-grey)",
                      padding: "20px",
                      height: "50px",
                      maxWidth: "150px",
                      overflow: "hidden",
                    }}
                    key={"player_" + index}
                  >
                    {player.username}
                  </div>
                  <div
                    style={{
                      fontSize: "small",
                    }}
                  >
                    <span
                      style={{
                        color: player.hasRound ? "white" : "var(--color-grey)",
                      }}
                    >
                      Points: {player.points} |{" "}
                    </span>
                    <span
                      style={{
                        color: player.hasRound ? "white" : "var(--color-grey)",
                      }}
                    >
                      {new Date((playerTimes as any)[player.userId] * 1000)
                        .toISOString()
                        .substring(14, 19)}
                    </span>
                  </div>
                </th>
              );
            })}
          </tr>
          {cells.map((x, index) => {
            return (
              <tr title={x.description} key={`cell_${index}`}>
                <td>{x.name}</td>
                {lobby.playerCells.map((cell, index) => {
                  return (
                    <td
                      key={"kk_" + index}
                      onClick={() => OnPointSet(x.pointType)}
                      className={GetCellClass(
                        lobby.playerCells,
                        cell.userId,
                        x.pointType
                      )}
                    >
                      {GetCell(cell.userId, x.pointType)}
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>
      <div className="dice-form">
        {lobby.gameStarted &&
          !gameEnded &&
          dices.map((dice, index) => {
            dice.index = index;
            return (
              <img
                alt="dice_img"
                onClick={() => SelectToRollDice(dice)}
                id={`dice_${index}`}
                key={`dice_${index}`}
                src={GetDiceImage(dice.rolledDots)}
              ></img>
            );
          })}
      </div>
      {gameEnded && (
        <div className="game-end">
          <h3>Game Ended</h3>
          <h3>Winner: {endgameScreen?.winnerUsername}</h3>
          <h3>
            Won:
            <span style={{ color: "var(--color-yellow)" }}>
              {Math.round(endgameScreen?.coinsGotten as number)}$
            </span>
          </h3>
        </div>
      )}
      <div className="roll-btn">
        {hasRound && lobby.gameStarted && !gameEnded && (
          <button onClick={() => setRollState(isRolling ? false : true)}>
            Rolls ({rollCount})
          </button>
        )}
        {!lobby.gameStarted && lobby.isCreator && (
          <button onClick={StartGame}>Start Game</button>
        )}
      </div>
    </div>
  );
};
