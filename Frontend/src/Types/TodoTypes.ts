export enum Progression {
  Done = "Done",
  InProgress = "In Progress",
  Hold = "Hold",
}

export interface TodoModel {
  Name: string;
  ProgressId: number;
  Date: string;
  Id: number;
}
