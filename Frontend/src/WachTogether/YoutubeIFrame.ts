class YoutubeIFrame {
  player: any;
  isEmpty: boolean;
  videoId?: string;

  onPlayerPauseChange?: (isPaused: boolean) => void;

  private seekTo_After?: number = undefined;

  private _isPaused: boolean = true;

  isInitialized: boolean = false;

  private onPlayerStateChange = (event: any) => {
    if (event.data === 1 || event.data === 2) {
      if (!this.isInitialized) {
        this.isInitialized = true;
      } else {
        if (typeof this.onPlayerPauseChange !== "undefined") {
          this.onPlayerPauseChange(event.data === 2);
        }

        this._isPaused = event.data === 2;
      }
    }
  };

  constructor() {
    this.isEmpty = true;
  }

  AttachApi() {
    var tag = document.createElement("script");
    tag.src = "https://www.youtube.com/iframe_api";
    var firstScriptTag = document.getElementsByTagName("script")[0];
    firstScriptTag.parentNode?.insertBefore(tag, firstScriptTag);
    console.log("iframe_api attached");
  }

  Build(videoId: string, onReady: (player: any) => void) {
    this.videoId = videoId;
    setTimeout(() => {
      this.player = new (window as any).YT.Player("player", {
        height: "520",
        width: window.innerWidth/2,
        playerVars: {
          html5: 1,
          disablekb: 0,
          fs: 1,
          controls: 0,
          modestbranding: 1,
        },
        videoId: videoId,
        events: {
          onReady: (event: any) => {
            this.isEmpty = false;

            if (typeof this.seekTo_After !== "undefined") {
              this.seekTo(this.seekTo_After);
            }

            onReady(event.target);
          },
          onStateChange: this.onPlayerStateChange,
        },
      });
    }, 200);
  }

  seekTo(time: number) {
    if (this.isEmpty) {
 
      this.seekTo_After = time;
      return;
    }
    console.log(this.player.getPlayerState())
    this.player.seekTo(time);
  }

  pauseVideo() {
    if (this.isEmpty) {
      return;
    }
    this._isPaused = true;
    this.player.pauseVideo();
  }

  playVideo() {
    this._isPaused = false;
    this.player.playVideo();
  }

  get isPlayerEmpty(): boolean {
    return typeof this.player === "undefined";
  }

  get videoUrl(): string {
    return this.player.playerInfo.videoUrl;
  }

  get DurationTimeAsTime(): string {
    let m = Math.floor(this.duration % 3600 / 60);
    let s = Math.floor(this.duration % 3600 % 60);

    let mDisplay = m <= 9 ? '0'+ m+':' : m+ ":";
    let sDisplay = s <= 9 ? '0'+ s : s;
    
    return mDisplay + sDisplay; 
  }

  get CurrentTimeAsTime(): string{
    let m = Math.floor(this.currentTime % 3600 / 60);
    let s = Math.floor(this.currentTime % 3600 % 60);

    let mDisplay = m <= 9 ? '0'+ m+':' : m+ ":";
    let sDisplay = s <= 9 ? '0'+ s : s;

    return mDisplay + sDisplay; 
  }

  get currentTime(): number {
    if (
      this.isPlayerEmpty ||
      typeof this.player.playerInfo.currentTime === "undefined"
    ) {
      return 0;
    }
    return this.player.playerInfo.currentTime as number;
  }

  get isPaused(): boolean {
    if (this.isEmpty) {
      return false;
    }

    return this._isPaused;
  }

  set isPaused(state: boolean) {
    this._isPaused = state;

    if (typeof this.onPlayerPauseChange !== "undefined") {
      this.onPlayerPauseChange(state);
    }
    if (state === false) {
      this.playVideo();
    } else if (state === true) {
      this.pauseVideo();
    }
  }

  get duration(): number {
    if (this.isEmpty) {
      return -1;
    }

    return this.player.getDuration() as number;
  }
}

export default YoutubeIFrame;