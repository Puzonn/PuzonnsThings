import axios from "axios";
import { useContext, useEffect, useState } from "react";
import { Auth } from "../Auth/Auth";
import {
  Progression,
  TaskModel,
  TaskSearchOptionType,
  ParseTask,
  GetMeridian,
} from "../Types/TodoTypes";
import "./Todo.css";
import { Config } from "../Shared/Config";
import { TodoSearchPanel } from "./TodoSearchPanel";
import { TodoTaskList } from "./TodoTaskList";
import { TodoCreator } from "./TodoCreator";
import { AuthContext } from "../Shared/AuthContext";

const Todo = () => {
  const { isLoggedIn, isFetched } = useContext(AuthContext);

  const [inputValue, setInputValue] = useState<string>("");
  const [fetchedTasks, setFetchedTasks] = useState<TaskModel[]>([]);
  const [screenWidth, setScreenWidth] = useState<number>(window.innerWidth);
  const [showMenuIcon, setShowMenuIcon] = useState<boolean>(false);
  const [searchOptionsSelected, setSearchOptionsSelected] =
    useState<TaskSearchOptionType>(TaskSearchOptionType.ByDate);
  const [selectedTask, setSelectedTask] = useState<TaskModel | undefined>();

  const [areTasksFetched, setTasksFetched] = useState<boolean>(false);
  const [showCreator, setShowCreator] = useState<boolean>(true);

  const IsMobile = screenWidth <= 768;
  const MobileShowMenu = IsMobile && showMenuIcon;
  const ShouldShowMenu = MobileShowMenu || !IsMobile;

  useEffect(() => {
    window.addEventListener(
      "resize",
      (event) => {
        setScreenWidth(window.innerWidth);
      },
      true
    );
  }, []);

  useEffect(() => {
    if (isLoggedIn && isFetched) {
      FetchTodoList();
    } else if (!isLoggedIn && isFetched) {
      window.location.href = "/login";
    }
  }, [isLoggedIn, isFetched]);

  useEffect(() => {
    if (selectedTask !== undefined) {
      setShowCreator(false);
    }
  }, [selectedTask]);

  useEffect(() => {
    const taskArr = fetchedTasks.slice();
    const sorted: TaskModel[] = [];

    const groupedTasks: { [key in Progression]: TaskModel[] } = {
      [Progression.Completed]: [],
      [Progression.InHold]: [],
      [Progression.Uncompleted]: [],
    };

    for (const task of fetchedTasks) {
      groupedTasks[task.ProgressId as Progression].push(task);
    }

    if (searchOptionsSelected === TaskSearchOptionType.ByDate) {
      taskArr.sort((a: TaskModel, b: TaskModel) => {
        a.Renderable = true;
        b.Renderable = true;
        return a.DateTime.getTime() - b.DateTime.getTime();
      });

      setFetchedTasks(taskArr);

      return;
    }

    if (searchOptionsSelected === TaskSearchOptionType.Today) {
      const now = new Date();

      for (const task of taskArr) {
        if (task.DateTime.getDate() !== now.getDate()) {
          task.Renderable = false;
        }
      }

      setFetchedTasks(taskArr);

      return;
    }

    if (
      searchOptionsSelected === TaskSearchOptionType.Completed ||
      searchOptionsSelected === TaskSearchOptionType.Uncompleted ||
      searchOptionsSelected === TaskSearchOptionType.InHold
    ) {
      let progressionOption: Progression;

      if (searchOptionsSelected === TaskSearchOptionType.Completed) {
        progressionOption = Progression.Completed;
      } else if (searchOptionsSelected === TaskSearchOptionType.Uncompleted) {
        progressionOption = Progression.Uncompleted;
      } else {
        progressionOption = Progression.InHold;
      }

      for (const key of Object.keys(groupedTasks)) {
        const intKey = parseInt(key);
        for (const task of groupedTasks[intKey as Progression]) {
          task.Renderable = true;
          if (progressionOption === task.ProgressId) {
            sorted.unshift(task);
          } else {
            sorted.push(task);
          }
        }
      }
    }

    setFetchedTasks(sorted);
  }, [searchOptionsSelected]);

  const FetchTodoList = () => {
    axios
      .get(Config.GetApiUrl() + "/api/todo/fetch", {
        headers: {
          "Content-Type": "application/json",
          Authorization: Auth.GetAuthorizationHeader(),
        },
      })
      .then((response) => {
        const tasks = response.data;
        const taskArr: TaskModel[] = [];

        for (let task of tasks) {
          taskArr.push(ParseTask(task));
        }

        setFetchedTasks(taskArr);
        setTasksFetched(true);
      });
  };

  const HandleTaskClick = (task: TaskModel | undefined) => {
    if (typeof selectedTask !== "undefined") {
      selectedTask.Selected = false;
    }

    if (typeof task !== "undefined") {
      task.Selected = true;
    }

    setSelectedTask(task);
  };

  return (
    <div id="todo-container">
      <TodoSearchPanel
        ShouldShowMenu={ShouldShowMenu}
        IsMobile={IsMobile}
        SearchOptionsSelected={searchOptionsSelected}
        SetSearchOption={setSearchOptionsSelected}
      />
      <TodoTaskList
        Tasks={fetchedTasks}
        IsFetched={isFetched}
        OnTaskClicked={HandleTaskClick}
      />
      <TodoCreator
        Tasks={fetchedTasks}
        SelectedTask={selectedTask}
        SetSelectedTask={setSelectedTask}
        ShowCreator={showCreator}
        SetTasks={setFetchedTasks}
        SetShowCreator={setShowCreator}
      />
    </div>
  );
};

export default Todo;
