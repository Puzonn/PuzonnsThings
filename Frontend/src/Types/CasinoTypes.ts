export interface RollOption {
  color: string;
  name: string;
  pointType: WheelPointType;
  isBetted: boolean;
  userBet?: number  
  playerBets: PlayerBet[]
  highestBet? : PlayerBet;
}

export interface PlayerBet {
  username: string;
  amount: number;
}

export interface OnPlayerBet {
  username: string;
  amount: number;
  wheelPoint: WheelPointType;
}

export interface WheelPoint {
  color: string;
  trueColor: string;
  rotateDeg: number;
}

export enum WheelPointType {
  red = 0,
  grey = 1,
  blue = 2,
  yellow = 3
}

export interface WheelBetModel {
  pointType: WheelPointType;
  amount: number;
}

export interface WheelBetCallbackModel {
  amount: number;
  success: boolean;
}

export interface WheelOnJoin {
  rollTimestamp: number;
  allBets: OnPlayerBet[];
  userBets: OnPlayerBet[];
}

export interface WheelOnRoll {
  winnerPoint: WheelPointType;
  nextRollTimestamp: number;
  cointsWon: number;
}