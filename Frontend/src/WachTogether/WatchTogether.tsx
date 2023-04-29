import axios from "axios";
import { useEffect, useState } from "react";
import "../App.css";
import { Auth } from "../Auth/Auth";
import { Room, RoomSync } from "../Types/WatchTogetherTypes";
import * as signalR from "@microsoft/signalr";
import YoutubeIFrame from "./YoutubeIFrame";
import { Base } from "../Shared/Config";

const hubConnection = new signalR.HubConnectionBuilder()
  .withUrl(Base.BASE_URL+"/services/watchtogether", {
    accessTokenFactory: () => Auth.GetAuthorizationToken(),
  })
  .build();

//USE PLAYER AS SINGELTON TO ALL OF INSTANCES OF THIS COMPONENT AS IT DOESNT NEED TO RERENDER ITS STATE
const player = new YoutubeIFrame();

export const WatchTogether = () => {
  const [roomInfo, setRoomInfo] = useState<Room>();
  const [isPlayerVisible, setPlayerVisibility] = useState<boolean>();
  const [urlInput, setUrlInput] = useState<string>();
  const [rooms, setRooms] = useState<Room[]>();
  const [isLoggedIn, setLoggedIn] = useState<boolean>(false);
  const [isCreator, setCreator] = useState<boolean>(false);

  const [isPlayerPaused, setPlayerPaused] = useState<boolean>(false);

  useEffect(() => {
    Auth.IsLoggedIn((response) => {
      if (!response) {
        window.location.href = "/login";
      }
      player.AttachApi();
      setLoggedIn(response);
    });
    FetchRooms();
  }, []);

  const GetVideoId = (value?: string) => {
    if (typeof value === "undefined") {
      return undefined;
    }

    const startIndex = value.indexOf("?v=") + 3;
    const endIndex = value.indexOf("&", startIndex);

    if (endIndex === -1) {
      return value.substring(startIndex);
    }
    return value.substring(startIndex, endIndex);
  };

  const OnRoomJoin = (room: Room) => {
    hubConnection.start().then((_) => {
      hubConnection.on("GetInfo", () => {
        const roomSync: RoomSync = {
          CurrentTime: player.currentTime,
          IsPaused: player.isPaused,
        };
        return roomSync;
      });

      hubConnection.on("SyncJoinState", (data) => {
        if (!data.IsPaused) {
          player.seekTo(Math.ceil(data.CurrentTime));
        }
      });

      hubConnection.on("ChangeState", (model) => {
        switch (model.state as string) {
          case "TimeSync":
            player.seekTo(model.data);
            break;
          case "PauseSync":
            player.isPaused = model.data === "true";
            break;
        }
      });

      hubConnection.on("CreatorJoined", () => {
        setCreator(true);
      });

      hubConnection.send("JoinRoom", room.RoomId);
    });

    setPlayerVisibility(true);

    SetPlayerView(room.VideoId, (success, _player) => {
      if (success) {
        setRoomInfo(room);
      } else {
        window.location.href = "/watchtogether";
      }
    });
  };

  const CreateRoom = (videoId: number, title: string, author: string) => {
    if (typeof videoId === "undefined" || typeof title === "undefined") {
      return;
    }

    const createModel = {
      VideoTitle: title,
      VideoAuthor: author,
      VideoId: videoId,
    };

    axios
      .post(
        Base.BASE_URL+"/api/WatchTogether/create",
        JSON.stringify(createModel),
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: Auth.GetAuthorizationHeader(),
          },
        }
      )
      .then((response) => {});
  };

  const FetchRooms = () => {
    axios
      .get(Base.BASE_URL+"/api/watchtogether/fetch", {
        headers: {
          "Content-Type": "application/json",
          Authorization: Auth.GetAuthorizationHeader(),
        },
      })
      .then((rooms) => {
        setRooms(rooms.data);
      });
  };

  const SetPlayerView = (
    videoId: string,
    callback: (success: boolean, player: any) => void
  ) => {
    player.Build(videoId, (event) => {
      CreateTimeControlLoop();
      callback(player.videoUrl !== "https://www.youtube.com/watch", event);
    });
    player.onPlayerPauseChange = (isPaused: boolean) => {
      setPlayerPaused(isPaused);
    };
  };

  const SubmitUrl = () => {
    const videoId = GetVideoId(urlInput);

    if (typeof videoId === "undefined") {
      return;
    }

    setPlayerVisibility(true);

    SetPlayerView(videoId, (success, player) => {
      if (success) {
        const videoData = player.playerInfo.videoData;
        CreateRoom(videoData.video_id, videoData.title, videoData.VideoAuthor);
      } else {
        window.location.href = "/watchtogether";
      }
    });
  };

  const JoinRoom = (roomId: number) => {
    axios
      .post(Base.BASE_URL+"/api/watchtogether/join?id=" + roomId, null, {
        headers: {
          Authorization: Auth.GetAuthorizationHeader(),
        },
      })
      .then((response) => {
        if (response.status === 200) {
          axios
            .get(Base.BASE_URL+"/api/watchtogether/get?id=" + roomId, {
              headers: {
                "Content-Type": "application/json",
                Authorization: Auth.GetAuthorizationHeader(),
              },
            })
            .then((response) => {
              if (response.status === 200) {
                setPlayerVisibility(true);
                OnRoomJoin(response.data);
              }
            });
        }
      });
  };

  const TimeControlClicked = (
    event: React.MouseEvent<HTMLDivElement, MouseEvent>
  ) => {
    const timeBar = (
      document.getElementById("lld") as any
    ).getBoundingClientRect();

    const rect = (event.target as any).getBoundingClientRect();
    const timeX = event.clientX - rect.left;
    const _x = (timeX * 100) / timeBar.width / 100;
    SetTimeControlWidth(timeX);
    const ytTime = _x * player.duration;

    player.seekTo(ytTime);

    SendState("TimeSync", ytTime.toString());
  };

  const SetTimeControlWidth = (timeWidth: number) => {
    const timeElement = document.getElementById("lla");
    const timeSpan = document.getElementById("lltime");

    if (timeElement === null || timeSpan === null) {
      return;
    }
    timeElement.style.width = timeWidth.toString() + "px";
    timeSpan.innerHTML = `${player.CurrentTimeAsTime} / ${player.DurationTimeAsTime}`
  };

  const CreateTimeControlLoop = () => {
    setInterval(() => {
      if (player.isPaused) {
        return;
      }
      const prctyt = (player.currentTime * 100) / player.duration;
      const width = (
        document.getElementById("lld") as any
      ).getBoundingClientRect().width;
      SetTimeControlWidth((prctyt * width) / 100);
    }, 1100);
  };

  const ChangeVideoPauseState = () => {
    if (!player.isInitialized) {
      SendState("PauseSync", `${false}`);
      return;
    }
    if (player.isPaused) {
      SendState("PauseSync", `${false}`);
    } else {
      SendState("PauseSync", `${true}`);
    }
  };

  const SendState = (state: string, data: string) => {
    hubConnection.send("SyncState", { State: state, Data: data });
  };

  if (!isPlayerVisible) {
    return (
      <div style={{ textAlign: "center" }}>
        {isLoggedIn && (
          <div>
            <input
              onChange={(x) => setUrlInput(x.target.value)}
              name="username"
              required
              placeholder="Youtube Url"
              type="text"
            />
            <div>
              <button onClick={() => SubmitUrl()}>Create Room</button>
            </div>
          </div>
        )}
        <h2 style={{ color: "white" }}>Rooms</h2>
        <table style={{ color: "white" }}>
          <tbody>
            <tr>
              <th>Room Id</th>
              <th>Room Creator</th>
              <th>Video Title</th>
              <th>Watchers Count</th>
              <th>Actions</th>
            </tr>
            {rooms?.map((room, index) => {
              return (
                <tr key={"room_" + index}>
                  <td>{room.RoomId}</td>
                  <td>{room.RoomCreator}</td>
                  <td style={{ maxWidth: "300px" }}>{room.VideoTitle}</td>
                  <td>{room.RoomWatchers}</td>
                  <td>
                    {isLoggedIn && (
                      <button
                        onClick={() => JoinRoom(room.RoomId)}
                        style={{ width: "150%" }}
                      >
                        Join
                      </button>
                    )}
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
    <div key="container" style={{ textAlign: "center" }}>
      <div id="player"></div>
      <div className="ll">
        <div id="lld" onClick={TimeControlClicked} className="lld"></div>
        <div id="lla" onClick={TimeControlClicked} className="lla"></div>
      </div>
      <span id="lltime" style={{margin: "10px", height: "10px"}}>{player.currentTime}/{player.duration}</span>
      {isCreator && (
        <div>
          <button onClick={ChangeVideoPauseState}>
            {isPlayerPaused ? "Start" : "Pause"}
          </button>
        </div>
      )}
    </div>
  );
};
