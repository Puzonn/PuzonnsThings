import { useEffect, useState } from "react";
import {
  CreatorPriorityType,
  CreatorPriorityTypes,
  CreatorProgressType,
  CreatorProgressTypes,
  ParseTask,
  TaskModel,
  TodoCreatorProps,
} from "../Types/TodoTypes";
import axios from "axios";
import { Config } from "../Shared/Config";
import { Auth } from "../Auth/Auth";
import CloseIcon from "../Icons/icon_close.svg";

export const TodoCreator = ({
  Tasks,
  SetTasks,
  SelectedTask,
  SetSelectedTask,
  ShowCreator,
  SetShowCreator,
}: TodoCreatorProps) => {
  const [creatorInputName, setCreatorInputName] = useState("");

  const [selectedTaskName, setSelectedTaskName] = useState("");
  const [selectedTaskEndDate, setSelectedTaskEndDate] = useState("");
  const [selectedTaskEndTime, setSelectedTaskEndTime] = useState("");
  const [selectedTaskPriority, setSelectedTaskPriority] =
    useState<CreatorPriorityType>(CreatorPriorityTypes[0]);
  const [selectedProgress, setSelectedTaskProgress] =
    useState<CreatorProgressType>(CreatorProgressTypes[0]);

  useEffect(() => {
    HandleSelectedTask();
  }, [SelectedTask]);

  useEffect(() => {
    SetDefaultInputs();
  }, []);

  useEffect(() => {
    if (ShowCreator && typeof SelectedTask !== "undefined") {
      SelectedTask.Selected = false;
      const tasksArr = Tasks.slice();

      const selectedTaskIndex = tasksArr.findIndex(
        (x) => x.Id === SelectedTask.Id
      );
      if (selectedTaskIndex === -1) {
        console.error(`Can't find selected task with given id`, tasksArr);

        if (ShowCreator) {
          SetDefaultInputs();
        }

        return;
      }
      Tasks[selectedTaskIndex].Selected = false;

      SetSelectedTask(SelectedTask);
      SetTasks(tasksArr);
    }
    if (ShowCreator) {
      SetDefaultInputs();
    }
  }, [ShowCreator]);

  const HandleTaskUpdate = () => {
    const date = new Date(`${selectedTaskEndDate} ${selectedTaskEndTime}`);

    const taskModelUpdate = {
      TaskName: selectedTaskName,
      taskPriority: selectedTaskPriority.Value,
      taskEndDateTime: date,
      TaskProgressId: selectedProgress.Value,
      TaskId: SelectedTask?.Id,
    };

    axios
      .put(
        Config.GetApiUrl() + "/api/todo/update",
        JSON.stringify(taskModelUpdate),
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: Auth.GetAuthorizationHeader(),
          },
        }
      )
      .then((response) => {
        if (response.status === 200) {
          const taskArr = Tasks.slice();

          const task = ParseTask(response.data);

          task.Selected = true;

          taskArr[taskArr.findIndex((x) => x.Id === response.data.Id)] = task;

          SetTasks(taskArr);
        }
      });
  };

  const HandleTaskDelete = (task?: TaskModel) => {
    if (task === null || typeof task === "undefined") {
      return;
    }

    axios
      .delete(Config.GetApiUrl() + "/api/todo/delete", {
        data: { id: task.Id },
        headers: {
          "Content-Type": "application/json",
          Authorization: Auth.GetAuthorizationHeader(),
        },
      })
      .then((response) => {
        if (response.status === 200) {
          const tasks = Tasks.slice();
          const todoIndex: number = tasks.findIndex((x) => x.Id === task.Id);
          tasks.splice(todoIndex, 1);
          SetTasks(tasks);
          SetShowCreator(true);
        }
      });
  };

  const HandleHandleTaskCreation = (event: any) => {
    event.preventDefault();

    const taskName = event.target.elements["task-name"].value;
    const taskPriority = event.target.elements["task-priority"].value;
    const taskEndDate = event.target.elements["task-end-date"].value;
    const taskEndTime = event.target.elements["task-end-time"].value;

    const date = new Date(`${taskEndDate} ${taskEndTime}`);

    const taskModelCreation = {
      TaskName: taskName,
      taskPriority: taskPriority,
      taskEndDateTime: date,
    };

    axios
      .post(
        Config.GetApiUrl() + "/api/todo/create",
        JSON.stringify(taskModelCreation),
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: Auth.GetAuthorizationHeader(),
          },
        }
      )
      .then((response) => {
        if (response.status === 200) {
          const tasks = Tasks.slice();
          tasks.push(ParseTask(response.data));
          SetTasks(tasks);
        }
      });

    setCreatorInputName("");
    SetDefaultInputs();
  };

  const SetDefaultInputs = () => {
    const datePicker = document.getElementById("todo-creator-date_picker");
    const timePicker = document.getElementById("todo-creator-time_picker");

    if (timePicker === null || datePicker === null) {
      return;
    }

    const now = new Date();
    const formattedTime = `${now.getHours().toString().padStart(2, "0")}:${now
      .getMinutes()
      .toString()
      .padStart(2, "0")}`;

    (datePicker as any).valueAsDate = now;
    (timePicker as any).value = formattedTime;
  };

  const HandleClose = () => {
    SetShowCreator(true);

    if (typeof SelectedTask !== "undefined") {
      SelectedTask.Selected = false;
    }

    SetSelectedTask(undefined);
  };

  const HandleSelectedTask = () => {
    if (SelectedTask === undefined) {
      return;
    }

    setSelectedTaskEndDate(SelectedTask.EndDate);
    setSelectedTaskEndTime(SelectedTask.EndTime);
    setSelectedTaskName(SelectedTask.Name);
    setSelectedTaskPriority(CreatorPriorityTypes[SelectedTask.Priority]);
    setSelectedTaskProgress(CreatorProgressTypes[SelectedTask.ProgressId]);
  };

  return (
    <div>
      {ShowCreator && (
        <form id="todo-creator" onSubmit={HandleHandleTaskCreation}>
          <h2>Create Task</h2>
          <div className="todo-creator_selector">
            <input
              style={{ marginBottom: "10px", marginLeft: "0px" }}
              name="task-name"
              placeholder="What's your next step?"
              onChange={(event) => setCreatorInputName(event.target.value)}
              id="todo-input"
              type="text"
              value={creatorInputName}
              autoComplete="off"
            ></input>
          </div>
          <div className="todo-creator_selector">
            <div>
              <span>Priority</span>
            </div>
            <select
              style={{
                color: `var(${selectedTaskPriority.Color})`,
                margin: "10px",
              }}
              onChange={(event) => {
                setSelectedTaskPriority(
                  CreatorPriorityTypes[parseInt(event.target.value)]
                );
              }}
              name="task-priority"
            >
              {CreatorPriorityTypes.map((priority, index) => {
                return (
                  <option
                    style={{ color: `var(${priority.Color})` }}
                    key={`PriorityType_${index}`}
                    value={priority.Value}
                  >
                    {priority.PriorityName}
                  </option>
                );
              })}
            </select>
          </div>
          <div className="todo-creator_selector">
            <div>
              <span>Due Date</span>
            </div>
            <input
              style={{ margin: "10px" }}
              required
              name="task-end-date"
              id="todo-creator-date_picker"
              type="date"
            />
            <input
              style={{ margin: "10px" }}
              required
              name="task-end-time"
              id="todo-creator-time_picker"
              type="time"
            />
          </div>
          <div style={{ textAlign: "center" }}>
            <button type="submit">Create Task</button>
          </div>
        </form>
      )}
      {!ShowCreator && (
        <div id="todo-creator">
          <h2>Update Task</h2>
          <img
            onClick={HandleClose}
            id="todo-creator-close_icon"
            src={CloseIcon}
          ></img>
          <div className="todo-creator_selector">
            <input
              style={{ marginBottom: "10px", marginLeft: "0px" }}
              placeholder="Name of task"
              onChange={(event) => setSelectedTaskName(event.target.value)}
              id="todo-input"
              type="text"
              value={selectedTaskName}
              autoComplete="off"
            ></input>
          </div>
          <div className="todo-creator_selector">
            <div>
              <span>Priority</span>
            </div>
            <select
              onChange={(event) =>
                setSelectedTaskPriority(
                  CreatorPriorityTypes[parseInt(event.target.value)]
                )
              }
              value={selectedTaskPriority.Value}
              style={{
                color: `var(${selectedTaskPriority.Color})`,
                margin: "10px",
              }}
            >
              {CreatorPriorityTypes.map((priority, index) => {
                return (
                  <option
                    style={{ color: `var(${priority.Color})` }}
                    key={`PriorityType_${index}`}
                    value={priority.Value}
                  >
                    {priority.PriorityName}
                  </option>
                );
              })}
            </select>
          </div>
          <div className="todo-creator_selector">
            <div>
              <span>Progress</span>
            </div>
            <select
              style={{
                color: `var(${selectedProgress.Color})`,
                margin: "10px",
              }}
              value={selectedProgress.Value}
              onChange={(event) =>
                setSelectedTaskProgress(
                  CreatorProgressTypes[parseInt(event.target.value)]
                )
              }
            >
              {CreatorProgressTypes.map((option, index) => {
                return (
                  <option
                    key={`ProgressType_${index}`}
                    style={{ color: `var(${option.Color})` }}
                    value={index}
                  >
                    {option.ProgressName}
                  </option>
                );
              })}
            </select>
          </div>
          <div className="todo-creator_selector">
            <div>
              <span>Due Date</span>
            </div>
            <input
              style={{ margin: "10px" }}
              value={selectedTaskEndDate}
              onChange={(event) => setSelectedTaskEndDate(event.target.value)}
              id="todo-creator-date_picker"
              type="date"
            />
            <input
              style={{ margin: "10px" }}
              value={selectedTaskEndTime}
              onChange={(event) => setSelectedTaskEndTime(event.target.value)}
              id="todo-creator-time_picker"
              type="time"
            />
          </div>

          <div className="todo-creator-update-buttons">
            <button
              style={{ color: "#DC143C" }}
              onClick={() => HandleTaskDelete(SelectedTask)}
            >
              Delete Task
            </button>
            <button onClick={HandleTaskUpdate}>Update Task</button>
          </div>
        </div>
      )}
    </div>
  );
};
