import { useEffect, useState } from "react";
import "./Yahtzee.css";
import {
  Dice,
  PointType,
  CellPoint,
  RoomModel,
  Player,
  PlayerCell,
  PointCell,
} from "../Types/YahtzeeTypes";
import * as signalR from "@microsoft/signalr";
import { Base } from "../Shared/Config";
import { Auth } from "../Auth/Auth";
import axios from "axios";
import Cookies from "js-cookie";
import { useSearchParams } from "react-router-dom";

const hubConnection = new signalR.HubConnectionBuilder()
  .withUrl(Base.BASE_URL + "/services/yahtzeeservice", {
    accessTokenFactory: () => Auth.GetAuthorizationToken(),
  })
  .build();

export const Yahtzee = () => {
  const cachedUsername = Cookies.get("Username");
  const CurrentPlayerName = cachedUsername
    ? cachedUsername
    : "<undefined username>";

  const [dices, setDices] = useState<Dice[]>([]);
  const [isRolling, setRollState] = useState<boolean>(false);
  const [selectedDices, setSelectedDices] = useState<number[]>([]);
  const [pointCells, setPointCells] = useState<PlayerCell[]>([]);
  const [rooms, setRooms] = useState<RoomModel[]>([]);
  const [isPlaying, setIsPlaying] = useState<boolean>(false);
  const [hasRound, setHasRound] = useState<boolean>(false);
  const [isCreator, setCreator] = useState<boolean>(false);
  const [gameStarted, setGameStarted] = useState<boolean>(false);
  const [players, setPlayers] = useState<Player[]>([]);
  const [rollCount, setRollCount] = useState<number>(2);
  const [gameEnded, setGameEnded] = useState<boolean>(false);

  const [params] = useSearchParams();

  const cells: CellPoint[] = [
    { pointType: PointType.One, name: "Ones", description: "Score the sum of all dice showing the number 1." },
    { pointType: PointType.Two, name: "Twos", description: "Score the sum of all dice showing the number 2." },
    { pointType: PointType.Three, name: "Threes", description: "Score the sum of all dice showing the number 3." },
    { pointType: PointType.Four, name: "Fours", description: "Score the sum of all dice showing the number 4." },
    { pointType: PointType.Five, name: "Fives", description: "Score the sum of all dice showing the number 5." },
    { pointType: PointType.Six, name: "Sixes", description: "Score the sum of all dice showing the number 6." },
    { pointType: PointType.ThreeOfaKind, name: "Three Of a Kind", description: "Score the sum of all five dice if at least three of them show the same number." },
    { pointType: PointType.FourOfaKind, name: "Four Of a Kind", description: "Score the sum of all five dice if at least four of them show the same number." },
    { pointType: PointType.SmallStraight, name: "Small Straight", description: "Score 30 points if the dice show a sequence of four numbers (for example, 1-2-3-4 or 2-3-4-5)." },
    { pointType: PointType.LargeStraight, name: "Large Straight", description: "Score 40 points if the dice show a sequence of five numbers (for example, 1-2-3-4-5 or 2-3-4-5-6)." },
    { pointType: PointType.Chance, name: "Chance", description: "Score the total sum of all five dice, regardless of the combination." },
    { pointType: PointType.FullHouse, name: "Full House", description: "Score 25 points if three of the dice show one number and the other two dice show another number." },
    { pointType: PointType.Yahtzee, name: "Yahtzee", description: "Score 50 points if all five dice show the same number." },
  ];

  const GetPointsFromType = (playerName: string, pointType: PointType) => {
    const point = pointCells.find((x) => x.playerName === playerName);

    if (typeof point === "undefined") {
      return "";
    }
    return point.pointCell.find(
      (x) => typeof x !== "undefined" && x.pointType === pointType
    )?.points;
  };

  const HasSetPoint = (playerName: string, pointType: PointType) => {
    const points = pointCells.find((x) => x.playerName === playerName);

    if (typeof points === "undefined") {
      return false;
    }

    if (typeof points.pointCell === "undefined") {
      return false;
    }

    for (const point of points.pointCell) {
      if (
        typeof point === "undefined" ||
        typeof point.pointType === "undefined"
      ) {
        continue;
      }
      if (point.pointType === pointType) {
        return true;
      }
    }

    return false;
  };

  const SetPointsFromType = (
    playerName: string,
    pointType: PointType,
    points: number
  ) => {
    const settedPoints = [...pointCells];

    const point = settedPoints.find((x) => x.playerName === playerName);

    if (typeof point === "undefined") {
      console.error(`Point of ${playerName} has undefined row ${pointType}`);
      return;
    }
    point.pointCell[pointType] = { pointType: pointType, points: points };
    setPointCells(settedPoints);
  };

  useEffect(() => {
    const idParm = params.get('id');

    if(idParm !== null){
      JoinRoom(parseInt(idParm))

      return;
    }

    FetchRooms();

    Auth.IsLoggedIn((suc) => {
      if (!suc) {
        window.location.href = "/login";
      }
    });
  }, []);

  const OnEndGame = () => {
    setGameEnded(true);
  };

  const OnJoin = (data: any) => {
    const cells: PlayerCell[] = [];
    
    for (const player of data.players) {
      const settedPoints: PointCell[] = [];
      for (const settedPoint of player.settedPoints) {
        settedPoints.push({
          pointType: settedPoint.point,
          points: settedPoint.pointsFromPoint,
        });
      }
      cells.push({ playerName: player.playerName, pointCell: settedPoints });
    }

    setPointCells(cells);

    if (data.hasRound) {
      setDices(data.dices);
    }

    setRollCount(data.rollCount);
    setGameStarted(data.gameStarted);
    setHasRound(data.hasRound);
    setCreator(data.isCreator);
    setPlayers(data.players);
    setDices(data.dices);
  };

  useEffect(() => {
    if (gameEnded) {
      return;
    }

    if (rollCount > 0 && gameStarted) {
      setRollCount(rollCount - 1);
      hubConnection.send("OnRollDices", selectedDices);
    }
  }, [isRolling]);

  useEffect(() => {}, [pointCells]);

  const CalculatePoints = (pointType: PointType) => {
    switch (pointType) {
      case PointType.One:
        return dices.filter((x) => x.rolledDots === 1).length;
      case PointType.Two:
        return dices.filter((x) => x.rolledDots === 2).length * 2;
      case PointType.Three:
        return dices.filter((x) => x.rolledDots === 3).length * 3;
      case PointType.Four:
        return dices.filter((x) => x.rolledDots === 4).length * 4;
      case PointType.Five:
        return dices.filter((x) => x.rolledDots === 5).length * 5;
      case PointType.Six:
        return dices.filter((x) => x.rolledDots === 6).length * 6;
      case PointType.ThreeOfaKind: {
        const counts: { [key: number]: number } = {};
        let sum = 0;
        for (const dice of dices) {
          counts[dice.rolledDots] = (counts[dice.rolledDots] || 0) + 1;
          sum += dice.rolledDots;
        }
        for (const count in counts) {
          if (counts[count] >= 3) {
            return sum;
          }
        }
        return 0;
      }
      case PointType.FourOfaKind: {
        const counts: { [key: number]: number } = {};
        let sum = 0;
        for (const dice of dices) {
          counts[dice.rolledDots] = (counts[dice.rolledDots] || 0) + 1;
          sum += dice.rolledDots;
        }
        for (const count in counts) {
          if (counts[count] >= 4) {
            return sum;
          }
        }
        return 0;
      }
      case PointType.SmallStraight: {
        let rolledDices: number[] = [];

        dices.forEach((roll) => {
          rolledDices.push(roll.rolledDots);
        });

        rolledDices = [...new Set(rolledDices)];

        rolledDices.sort((a, b) => a - b);

        if (rolledDices.length < 4) {
          return 0;
        }

        if (
          rolledDices[0] === 1 &&
          rolledDices[1] === 2 &&
          rolledDices[2] === 3 &&
          rolledDices[3] === 4
        ) {
          return 30;
        }
        if (
          rolledDices[1] === 2 &&
          rolledDices[2] === 3 &&
          rolledDices[3] === 4 &&
          rolledDices[4] === 5
        ) {
          return 30;
        }
        if (
          rolledDices[2] === 3 &&
          rolledDices[3] === 4 &&
          rolledDices[4] === 5 &&
          rolledDices[5] === 6
        ) {
          return 30;
        }

        return 0;
      }
      case PointType.LargeStraight: {
        let rolledDices: number[] = [];

        dices.forEach((roll) => {
          rolledDices.push(roll.rolledDots);
        });

        rolledDices = [...new Set(rolledDices)];
        rolledDices.sort((a, b) => a - b);

        if (
          rolledDices.toString() === "1,2,3,4,5" ||
          rolledDices.toString() === "2,3,4,5,6"
        ) {
          return 40;
        }
        return 0;
      }
      case PointType.Chance: {
        let sum = 0;

        for (let i = 0; i < dices.length; i++) {
          sum += dices[i].rolledDots;
        }

        return sum;
      }
      case PointType.Yahtzee: {
        const counts: { [key: number]: number } = {};
        for (const dice of dices) {
          counts[dice.rolledDots] = (counts[dice.rolledDots] || 0) + 1;
        }
        if (Object.keys(counts).length === 1) {
          return 50;
        }
        return 0;
      }
      case PointType.FullHouse: {
        const counts: { [key: number]: number } = {};
        for (const dice of dices) {
          counts[dice.rolledDots] = (counts[dice.rolledDots] || 0) + 1;
        }
        const frequencies = Object.values(counts);
        if (frequencies.includes(2) && frequencies.includes(3)) {
          return 25;
        }
        return 0;
      }
    }
  };

  const GetCell = (playerName: string, pointType: PointType) => {
    if (!gameStarted) {
      return "";
    }
    if (HasSetPoint(playerName, pointType)) {
      return GetPointsFromType(playerName, pointType);
    } else {
      if (!hasRound) {
        return "";
      }
      if (playerName === CurrentPlayerName) {
        return CalculatePoints(pointType);
      }
    }
  };

  const GetCellClass = (playerName: string, pointType: PointType) => {
    if (HasSetPoint(playerName, pointType)) {
      return "point-setted";
    }

    return "point-unsetted";
  };

  useEffect(() => {
    if (!gameStarted) {
      return;
    }
    for (let i = 0; i < dices.length; i++) {
      if (selectedDices.includes(dices[i].index)) {
        const diceElement = document.getElementById(`dice_${i}`);

        if (diceElement === null) {
          console.error('dice Element is null')
          continue;
        }

        diceElement.style.border = "1px solid red";
      } else {
        const diceElement = document.getElementById(`dice_${i}`);

        if (diceElement === null) {
          console.error('dice Element is null')
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
      const settedPoints: PointCell[] = [];

      for (const settedPoint of player.settedPoints) {
        settedPoints.push({
          pointType: settedPoint.point,
          points: settedPoint.pointsFromPoint,
        });
      }
      cells.push({ playerName: player.playerName, pointCell: settedPoints });
    }

    setPointCells(cells);
    setHasRound(data.hasRound);
    setPlayers(data.players);
  };

  const OnPointSet = (pointType: PointType) => {
    if (gameEnded) {
      return;
    }
    hubConnection.invoke("OnPointSet", pointType).then((x) => {
      if (!x.isSuccessFul) {
        return;
      }

      SetPointsFromType(CurrentPlayerName, x.point, x.pointsFromPoint);
    });
  };

  const GetDiceImage = (rolledNumber: number) => {
    switch (rolledNumber) {
      case 1:
        return require("./Assets/dice1.png");
      case 2:
        return require("./Assets/dice2.png");
      case 3:
        return require("./Assets/dice3.png");
      case 4:
        return require("./Assets/dice4.png");
      case 5:
        return require("./Assets/dice5.png");
      case 6:
        return require("./Assets/dice6.png");
    }
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
      const diceArrayIndex = mSelectedDices.indexOf(dice.index, 0)

      mSelectedDices.splice(diceArrayIndex, 1)

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
      window.history.pushState('', '', '/yahtzee?id='+roomId)

      hubConnection.on("SetDices", setDices);
      hubConnection.on("OnJoin", OnJoin);
      hubConnection.on("OnNextRound", OnNextRound);
      hubConnection.on("OnEndGame", OnEndGame);

      hubConnection.invoke("Join", roomId).then((canJoin) => {
        if(!canJoin){
          console.log(canJoin)
          FetchRooms();
          hubConnection.stop();
        }
        else{
          console.log(canJoin)
          setIsPlaying(canJoin);
        }
      });
    });
  };

  const FetchRooms = async () => {
    const response = await axios.get(Base.BASE_URL + "/api/yahtzee/fetch", {
      headers: {
        Authorization: Auth.GetAuthorizationHeader(),
      },
    });
    setRooms(response.data);
  };

  const StartGame = () => {
    hubConnection.invoke("StartGame");
  };

  const CreateRoom = async () => {
    await axios
      .post(
        Base.BASE_URL + "/api/yahtzee/create",
        {},
        {
          headers: {
            Authorization: Auth.GetAuthorizationHeader(),
          },
        }
      )
      .then(async (x) => {
        JoinRoom(x.data.id);
      });
  };

  if (!isPlaying) {
    return (
      <div style={{ textAlign: "center" }}>
        <div>
          <div>
            <button onClick={CreateRoom}>Create Room</button>
          </div>
        </div>
        <h2 style={{ color: "white" }}>Rooms</h2>
        <table style={{ color: "white" }}>
          <tbody>
            <tr>
              <th>Room Id</th>
              <th>Room Creator</th>
              <th></th>
            </tr>
            {rooms.map((room, index) => {
              return (
                <tr key={"room_" + index}>
                  <td>{room.id}</td>
                  <td>{room.creator}</td>
                  <td>
                    <button
                      onClick={() => JoinRoom(room.id)}
                      style={{ width: "150%" }}
                    >
                      Join
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    );
  }

  return (
    <div>
      <table>
        <tbody>
          <tr>
            <th></th>
            {players.map((player, index) => {
              return (
                <th
                  style={{ color: player.hasRound ? "white" : "grey" }}
                  key={"player_" + index}
                >
                  {player.playerName} / {player.points}
                </th>
              );
            })}
          </tr>
          {cells.map((x, index) => {
            return (
              <tr title={x.description} key={`cell ${index}`}>
                <td>{x.name}</td>
                {pointCells.map((cell, index) => {
                  return (
                    <td
                      key={"kk_" + index}
                      onClick={() => OnPointSet(x.pointType)}
                      className={GetCellClass(cell.playerName, x.pointType)}
                    >
                      {GetCell(cell.playerName, x.pointType)}
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>
      <div className="dice-form">
        {gameStarted &&
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
        </div>
      )}
      <div className="roll-btn">
        {hasRound && gameStarted && !gameEnded && (
          <button onClick={() => setRollState(isRolling ? false : true)}>
            Rolls ({rollCount})
          </button>
        )}
        {!gameStarted && isCreator && (
          <button onClick={StartGame}>Start Game</button>
        )}
      </div>
    </div>
  );
};
