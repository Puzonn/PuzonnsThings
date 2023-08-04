import { Dice, PointType } from "../Types/YahtzeeTypes";

export const CalculatePoints = (dices: Dice[], pointType: PointType) => {
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
