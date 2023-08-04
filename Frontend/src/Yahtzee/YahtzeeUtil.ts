import { PlayerCell, PointType } from "../Types/YahtzeeTypes";

export const HasSetPoint = (
  pointCells: PlayerCell[],
  userId: number,
  pointType: PointType
) => {
  const points = pointCells.find((x) => x.userId === userId);

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

export const GetDiceImage = (RolledNumber: number) => {
  switch (RolledNumber) {
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

export const GetCellClass = (
  pointCells: PlayerCell[],
  userId: number,
  pointType: PointType
) => {
  if (HasSetPoint(pointCells, userId, pointType)) {
    return "point-setted";
  }

  return "point-unsetted";
};

export const GetPointsFromType = (
  pointCells: PlayerCell[],
  userId: number,
  pointType: PointType
) => {
  const point = pointCells.find((x) => x.userId === userId);

  if (typeof point === "undefined") {
    return "";
  }
  return point.pointCell.find(
    (x) => typeof x !== "undefined" && x.pointType === pointType
  )?.points;
};
