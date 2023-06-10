const _Red = "--color-red";
const _Green = "--color-green";
const _Yellow = "--color-yellow";

export enum Progression {
  Completed,
  InHold,
  Uncompleted,
}

export interface TaskModel {
  Name: string;
  Renderable: boolean;
  Selected: boolean;
  ProgressId: number;
  Priority: number;
  EndDate: string;
  EndTime: string;
  Meridian: string;
  DateTime: Date;
  Id: number;
}

interface TaskSearchOption {
  SearchOptionType: TaskSearchOptionType;
  SearchName: string;
  Color?: string;
}

export enum TaskSearchOptionType {
  ByDate,
  Completed,
  InHold,
  Uncompleted,
  Today,
}

export const SearchOptions: TaskSearchOption[] = [
  { SearchOptionType: TaskSearchOptionType.ByDate, SearchName: "By Date" },
  {
    SearchOptionType: TaskSearchOptionType.Completed,
    SearchName: "Completed",
    Color: _Green,
  },
  {
    SearchOptionType: TaskSearchOptionType.InHold,
    SearchName: "In Hold",
    Color: _Yellow,
  },
  {
    SearchOptionType: TaskSearchOptionType.Uncompleted,
    SearchName: "Uncompleted",
    Color: _Red,
  },
  { SearchOptionType: TaskSearchOptionType.Today, SearchName: "Today" },
];

export interface TodoSearchPanelProps {
  ShouldShowMenu: boolean;
  IsMobile: boolean;
  SearchOptionsSelected: TaskSearchOptionType;
  SetSearchOption: (SearchOptions: TaskSearchOptionType) => void;
}

export interface TodoTaskListProps {
  Tasks: TaskModel[];
  IsFetched: boolean;
  OnTaskClicked: (task: TaskModel) => void;
}

export interface TodoCreatorProps {
  Tasks: TaskModel[];
  SelectedTask?: TaskModel;
  SetSelectedTask: (Task: TaskModel | undefined) => void;
  ShowCreator: boolean;
  SetShowCreator: (State: boolean) => void;
  SetTasks: (Tasks: TaskModel[]) => void;
}

export const ParseTask = (data: any) => {
  const date = new Date(data.TaskEndDateTime);
  const formattedEndTime = `${date
    .getHours()
    .toString()
    .padStart(2, "0")}:${date.getMinutes().toString().padStart(2, "0")}`;
  const formattedEndDate = `${date
    .getFullYear()
    .toString()
    .padStart(2, "0")}-${(date.getMonth() + 1)
    .toString()
    .padStart(2, "0")}-${date.getDate().toString().padStart(0, "0")}`;
  return {
    Name: data.TaskName,
    ProgressId: data.TaskProgressId,
    EndDate: formattedEndDate,
    EndTime: formattedEndTime,
    Priority: data.TaskPriority,
    Meridian: GetMeridian(date),
    Id: data.Id,
    DateTime: date,
    Renderable: true,
    Selected: false,
  };
};

export const GetMeridian = (time: Date) => {
  const hour = time.getHours();
  return hour >= 12 ? "PM" : "AM";
};

export interface CreatorProgressType {
  ProgressName: string;
  Color: string;
  Value: number;
}

export interface CreatorPriorityType {
  PriorityName: string;
  Color: string;
  Value: number;
}

export const CreatorPriorityTypes: CreatorPriorityType[] = [
  { PriorityName: "High", Color: _Red, Value: 0 },
  { PriorityName: "Medium", Color: _Yellow, Value: 1 },
  { PriorityName: "Low", Color: _Green, Value: 2 },
];

export const CreatorProgressTypes: CreatorProgressType[] = [
  { ProgressName: "Completed", Color: _Green, Value: 0 },
  { ProgressName: "In Hold", Color: _Yellow, Value: 1 },
  { ProgressName: "Uncompleted", Color: _Red, Value: 2 },
];
