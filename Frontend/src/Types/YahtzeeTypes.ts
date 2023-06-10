export interface Dice {
  rolledDots: number;
  index: number;
  isSelected: boolean;
}

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
  Chance
}

export interface CellPoint {
  pointType: PointType;
  name: string;
  description: string;
}

export interface RoomModel {
  id: number;
  creator: string;
}

export interface Player {
  playerName: string;
  points: number;
  hasRound: boolean;
}

export interface PointCell {
  pointType: PointType;
  points: number;
}

export interface PlayerCell {
  pointCell: PointCell[];
  playerName: string;
}

export interface Endgame {
  coinsGotten: number;
  winnerUsername: string;
}