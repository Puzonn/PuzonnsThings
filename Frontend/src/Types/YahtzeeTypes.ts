import { BooleanLiteral } from "typescript";

export type Dice = {
  rolledDots: number;
  index: number;
  isSelected: boolean;
};

export enum PointType {
  None,
  One,
  Two,
  Three,
  Four,
  Five,
  Six,
  ThreeOfaKind,
  FourOfaKind,
  FullHouse,
  SmallStraight,
  LargeStraight,
  Yahtzee,
  Chance,
}

export type CellPoint = {
  pointType: PointType;
  name: string;
  description: string;
};

export type RoomModel = {
  id: number;
  creator: string;
};

export type Options = {
  isPublic: boolean;
  maxPlayers: number;
  gameTime: number;
};

export type Player = {
  username: string;
  gameTime: number;
  points: number;
  hasTime: boolean;
  userId: number;
  hasRound: boolean;
  lobbyPlaceId: number | undefined;
};

export type PointCell = {
  pointType: PointType;
  points: number;
};

export type PlayerCell = {
  pointCell: PointCell[];
  userId: number;
};

export type GameTimer = {
  playerRound: number;
  gameTime: number;
  playersTimes: any[];
};

export type OptionsMaxPlayersState = {
  players: Player[];
  maxPlayersState: number;
};

export type Lobby = {
  players: Player[];
  lobbyId: number;
  isCreator: boolean;
  startState: boolean;
  options: Options;
  gameStarted: boolean;
  playerCells: PlayerCell[];
};

export type Endgame = {
  coinsGotten: number;
  winnerUsername: string;
};

export const PointCells: CellPoint[] = [
  {
    pointType: PointType.One,
    name: "Ones",
    description: "Score the sum of all dice showing the number 1.",
  },
  {
    pointType: PointType.Two,
    name: "Twos",
    description: "Score the sum of all dice showing the number 2.",
  },
  {
    pointType: PointType.Three,
    name: "Threes",
    description: "Score the sum of all dice showing the number 3.",
  },
  {
    pointType: PointType.Four,
    name: "Fours",
    description: "Score the sum of all dice showing the number 4.",
  },
  {
    pointType: PointType.Five,
    name: "Fives",
    description: "Score the sum of all dice showing the number 5.",
  },
  {
    pointType: PointType.Six,
    name: "Sixes",
    description: "Score the sum of all dice showing the number 6.",
  },
  {
    pointType: PointType.ThreeOfaKind,
    name: "Three Of a Kind",
    description:
      "Score the sum of all five dice if at least three of them show the same number.",
  },
  {
    pointType: PointType.FourOfaKind,
    name: "Four Of a Kind",
    description:
      "Score the sum of all five dice if at least four of them show the same number.",
  },
  {
    pointType: PointType.FullHouse,
    name: "Full House",
    description:
      "Score 25 points if three of the dice show one number and the other two dice show another number.",
  },
  {
    pointType: PointType.SmallStraight,
    name: "Small Straight",
    description:
      "Score 30 points if the dice show a sequence of four numbers (for example, 1-2-3-4 or 2-3-4-5).",
  },
  {
    pointType: PointType.LargeStraight,
    name: "Large Straight",
    description:
      "Score 40 points if the dice show a sequence of five numbers (for example, 1-2-3-4-5 or 2-3-4-5-6).",
  },
  {
    pointType: PointType.Chance,
    name: "Chance",
    description:
      "Score the total sum of all five dice, regardless of the combination.",
  },
  {
    pointType: PointType.Yahtzee,
    name: "Yahtzee",
    description: "Score 50 points if all five dice show the same number.",
  },
];
 