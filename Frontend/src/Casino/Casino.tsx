import { useContext, useEffect, useState } from "react";
import "./Casino.css";
import {
  RollOption,
  WheelBetModel,
  WheelOnJoin,
  WheelOnRoll,
  WheelPointType,
  WheelBetCallbackModel,
  OnPlayerBet,
} from "../Types/CasinoTypes";
import { WheelPoints } from "./WheelPoints";
import * as signalR from "@microsoft/signalr";
import { Config } from "../Shared/Config";
import { Auth } from "../Auth/Auth";
import { UserContext } from "../Shared/UserContext";

const hubConnection = new signalR.HubConnectionBuilder()
  .withUrl(Config.GetApiUrl() + "/services/wheelservice", {
    accessTokenFactory: () => Auth.GetAuthorizationToken(),
  })
  .configureLogging(signalR.LogLevel.Critical)
  .build();

export default () => {
  const user = useContext(UserContext);
  const [rollOptions, setRollOptions] = useState<RollOption[]>([
    {
      name: "2x",
      color: "#808080",
      pointType: WheelPointType.grey,
      isBetted: false,
      playerBets: [],
    },
    {
      name: "3x",
      color: "#D22B2B",
      pointType: WheelPointType.red,
      isBetted: false,
      playerBets: [],
    },
    {
      name: "5x",
      color: "#4FA1CA",
      pointType: WheelPointType.blue,
      isBetted: false,
      playerBets: [],
    },
    {
      name: "50x",
      color: "#E4D00A",
      pointType: WheelPointType.yellow,
      isBetted: false,
      playerBets: [],
    },
  ]);

  const [spinTimer, setSpinTimer] = useState(30);
  const [betInput, setBetInput] = useState("0");
  const [canBet, setCanBet] = useState(true);
  const [showMoreInfo, setShowMoreInfo] = useState(false);
  const [showPostRoll, setShowPostRoll] = useState(false);

  useEffect(() => {
    if (hubConnection.state !== signalR.HubConnectionState.Disconnected) {
      return;
    }
    hubConnection.start().then(() => {
      hubConnection.on("OnJoin", OnJoin);
      hubConnection.on("OnRoll", OnRoll);
      hubConnection.on("OnPlayerBet", OnPlayerBet);
    });
  }, []);

  const OnPlayerBet = (bet: OnPlayerBet) => {
    const options = [...rollOptions];
    const index = options.findIndex((x) => x.pointType === bet.wheelPoint);

    options[index].playerBets.push({
      username: bet.username,
      amount: bet.amount,
    });

    for (const option of options) {
      option.playerBets = option.playerBets.sort((x, y) => y.amount - x.amount);
    }

    setRollOptions(options);
  };

  const OnRoll = (rollEvent: WheelOnRoll) => {
    RotateTo(WheelPointType[rollEvent.winnerPoint]);
    setCanBet(false);
    setTimeout(() => {
      setCanBet(true);
    }, 4000);

    setSpinTimer(CalculateTimeFromTimestamp(rollEvent.nextRollTimestamp));
  };

  const CalculateTimeFromTimestamp = (timeStamp: number) => {
    const rollDate = new Date(timeStamp * 1000);
    const nowDate = new Date();
    return Math.floor((rollDate.getTime() - nowDate.getTime()) / 1000);
  };

  const InitWheelTimer = () => {
    let lastTime = 0;
    const timer = setInterval(() => {
      setSpinTimer((prevTime) => {
        if (lastTime < prevTime && lastTime !== 0) {
          console.warn(
            `Timer was out of sync where last time was: ${lastTime} and actual time is: ${prevTime}`
          );
        }

        lastTime = prevTime;
        return prevTime - 1;
      });
    }, 1000);
  };

  const OnJoin = (joinEvent: WheelOnJoin) => {
    setSpinTimer(CalculateTimeFromTimestamp(joinEvent.rollTimestamp));
    const options = [...rollOptions];

    for (const bet of joinEvent.allBets) {
      options[
        options.findIndex((x) => x.pointType === bet.wheelPoint)
      ].playerBets.push(bet);
    }

    for (const bet of joinEvent.userBets) {
      options[
        options.findIndex((x) => x.pointType === bet.wheelPoint)
      ].playerBets.push(bet);
      options[
        options.findIndex((x) => x.pointType === bet.wheelPoint)
      ].isBetted = true;
      options[
        options.findIndex((x) => x.pointType === bet.wheelPoint)
      ].userBet = bet.amount;
    }

    for (const option of options) {
      option.playerBets = option.playerBets.sort((x, y) => y.amount - x.amount);
    }

    setRollOptions(options);

    InitWheelTimer();
  };

  const RotateTo = (color: string) => {
    const degOffset = 15;
    const elements = document.getElementsByClassName(`wheel_tag_${color}`);
    const rndElement = elements[Math.floor(Math.random() * elements.length)];
    const rndElementRotate = parseInt(
      ((rndElement as any).style.rotate as string).slice(0, -3)
    );
    const rndElementRotateOffset = Math.floor(Math.random() * degOffset);
    const container = document.getElementById("ss-container");
    let angle = 0;
    let fakeAngle = 0;

    let rotations = 3 * 360;
    let angleModifier = 4;

    const maxDistance = rotations + rndElementRotate + rndElementRotateOffset;

    const deceleration = -(angleModifier ** 2 / (2 * maxDistance));

    const renderRotate = () => {
      fakeAngle += angleModifier;
      angle += angleModifier;

      if (container === null) {
        return;
      }

      container.style.transform = "rotate(" + angle + "deg)";

      if (rotations - fakeAngle <= 0) {
        if (maxDistance >= fakeAngle) {
          requestAnimationFrame(renderRotate);
        } else {
          ClearBets();
        }
      } else {
        requestAnimationFrame(renderRotate);
      }

      angleModifier = angleModifier + deceleration;
    };
    renderRotate();
  };

  const ClearBets = () => {
    const options = [...rollOptions];

    for (const option of options) {
      option.playerBets = [];
      option.isBetted = false;
    }

    setRollOptions(options);
  };

  const OnBetClicked = (option: RollOption) => {
    if (betInput === "" || betInput === "0") {
      return;
    }

    const bet: WheelBetModel = {
      amount: parseFloat(betInput),
      pointType: option.pointType,
    };

    hubConnection.invoke("Bet", bet).then((callback: WheelBetCallbackModel) => {
      if (callback.success) {
        const options = [...rollOptions];
        const index = options.findIndex(
          (x) => x.pointType === option.pointType
        );
        options[index].isBetted = true;
        setRollOptions(options);
      }
    });

    setShowPostRoll(true);
  };

  const InputAddBet = (count: number) => {
    const bet = parseFloat(betInput) + count;
    setBetInput(bet.toString());
  };

  const InputClearBet = () => {
    setBetInput("0");
  };

  const InputMaxBet = () => {
    setBetInput(user.Coins.toString());
  };

  const InputHalfBet = () => {
    const bet = parseInt(betInput) / 2;
    setBetInput(bet.toString());
  };

  const InputMultiplyBet = () => {
    const bet = parseInt(betInput) * 2;
    setBetInput(bet.toString());
  };

  return (
    <div id="container">
      <div id="ss">
        <div id="ss-timer">{showPostRoll && <span>{spinTimer}</span>}</div>
        <div id="ss-point"></div>
        <div>
          <WheelPoints />
          {true && (
            <div id="ss-modal">
              <div id="ss-modal-roll_info-container">
                <div id="ss-modal-roll_info-info">
                  <h3 id="ss-modal-roll_info-win">
                    You just won: {Math.floor(Math.random() * 100)} Points !
                  </h3>
                  {true && (
                    <div>
                      <span>Your bets: </span>
                      <ul style={{ textAlign: "initial" }}>
                        <li>Grey: {rollOptions[0].userBet} Points</li>
                        <li>Red:</li>
                        <li>Blue: </li>
                        <li>Yellow: </li>
                      </ul>
                    </div>
                  )}
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
      <div>
        <div id="ss-input">
          <input
            placeholder="bet"
            id="ss-cash-input"
            type="number"
            max={20}
            minLength={0}
            value={betInput}
            onChange={(e) => setBetInput(e.target.value)}
          ></input>
          <ul id="ss-bet-input">
            <li onClick={InputClearBet}>CLEAR</li>
            <li onClick={() => setBetInput("1")}>MIN</li>
            <li onClick={() => InputAddBet(1)}>+1</li>
            <li onClick={() => InputAddBet(10)}>+10</li>
            <li onClick={() => InputAddBet(100)}>+100</li>
            <li onClick={InputHalfBet}>X1/2</li>
            <li onClick={InputMultiplyBet}>X2</li>
            <li onClick={InputMaxBet}>MAX</li>
          </ul>
        </div>
        <ul id="ss-options">
          {rollOptions.map((x, index) => {
            return (
              <li onClick={() => OnBetClicked(x)} key={"roll_option" + index}>
                <span
                  style={{
                    border: `2px solid ${x.color}`,
                    borderRadius: "10px",
                    opacity: x.isBetted || !canBet ? "0.3" : "1",
                  }}
                >
                  Place Bet ({x.name})
                </span>
                <div>
                  <p style={{ color: x.color }} className="ss-bets-count">
                    Bets: {x.playerBets.length}
                  </p>
                </div>
                <div className="ss-bets">
                  <ul>
                    {x.playerBets.map((bet, index) => {
                      return (
                        <li
                          className="ss-bets-user"
                          key={`bet_${bet.username}_${index}`}
                        >
                          <span
                            style={{
                              float: "left",
                              maxWidth: "90px",
                              fontWeight: "normal",
                              color: x.color,
                            }}
                          >
                            {bet.username}
                          </span>
                          <span style={{ float: "right", color: x.color }}>
                            {bet.amount}
                          </span>
                        </li>
                      );
                    })}
                  </ul>
                </div>
              </li>
            );
          })}
        </ul>
      </div>
    </div>
  );
};
