import axios from "axios";
import { useEffect, useReducer, useState } from "react";
import { Auth } from "../Auth/Auth";
import { Progression, TodoModel } from "../Types/TodoTypes"
import './Todo.css'
import { Base } from "../Shared/Config";

function Todo() {
  const [inputValue, setInputValue] = useState<string>("");
  const [todos, setTodos] = useState<TodoModel[]>([]);
  const [todoUpdated, updateTodo] = useReducer((x) => x + 1, 0);
  const [completedCount, setCompletedCount] = useState<number>(0);

  const GetProgressId = (progress: Progression) => {
    return Object.values(Progression).findIndex((x) => x === progress);
  };

  const GetProgressName = (progressId: number) => {
    return Object.values(Progression)[progressId];
  };

  const GetProgressByIndex = (progressId: number) => {
    return Object.keys(Progression)[progressId] as Progression;
  };

  const OnInputChange = (value: any) => {
    setInputValue(value.target.value);
  };

  const OnInputEnter = (event: any) => {
    if (event.key === "Enter") {
      if (inputValue !== undefined && inputValue.length > 0) {
        const todosArr = todos.slice();
        if (todosArr.find((x) => x.Name === inputValue)) {
          return;
        }

        setInputValue("");

        const actualDate = new Date();
        const newTodo: TodoModel = {
          Id: todosArr.length,
          Name: inputValue,
          ProgressId: GetProgressId(Progression.InProgress),
          Date:
            actualDate.getDay() +
            "/" +
            actualDate.getMonth() +
            "/" +
            actualDate.getFullYear(),
        };
        PostTodo(newTodo);
      }
    }
  };

  const AddTodo = (todo: TodoModel) => {
    setTodos([...todos, todo]);
  };

  useEffect(() => {
    const completedCount = todos.filter(
      (x) => x.ProgressId === GetProgressId(Progression.Done)
    );
    setCompletedCount(completedCount.length);
  }, [todos, todoUpdated]);

  useEffect(() => {
    Auth.IsLoggedIn((response) => {
      if (!response) {
        console.log(response +" response from auth")
        window.location.href = "/login";
      } else {
        FetchTodoList();
      }
    });
  }, []);

  const HandleDelete = (todo: TodoModel) => {
    axios
      .delete(Base.BASE_URL+"/api/todo/delete", {
        data: { id: todo.Id },
        headers: 
        {
          "Content-Type": "application/json",
          "Authorization": Auth.GetAuthorizationHeader()
        },
      })
      .then((response) => console.log(response));

    const todoArr = todos.slice();
    const todoIndex: number = todoArr.findIndex((x) => x.Id === todo.Id);
    todoArr.splice(todoIndex, 1);
    setTodos(todoArr);
  };

  const GetProgressColor = (progress: Progression): string => {
    if (progress === Progression.Done) {
      return "#00ff73";
    } else if (progress === Progression.InProgress) {
      return "#FF4500";
    } else {
      return "#dc143c";
    }
  };

  const PostTodo = (todo: TodoModel) => {
    axios
      .post(Base.BASE_URL+"/api/todo/create", JSON.stringify(todo), {
        headers: 
        {
          "Content-Type": "application/json",
          "Authorization": Auth.GetAuthorizationHeader()
        },
      })
      .then((response) => {
        AddTodo(response.data);
      });
  };

  const HandleProgressClicked = (todo: TodoModel) => {
    if (todo.ProgressId === GetProgressId(Progression.InProgress)) {
      todo.ProgressId = GetProgressId(Progression.Done);
    } else if (todo.ProgressId === GetProgressId(Progression.Done)) {
      todo.ProgressId = GetProgressId(Progression.InProgress);
    }

    const todoUpdateMode = {
      TodoId: todo.Id,
      ProgressId: todo.ProgressId,
    };

    axios.put(
      Base.BASE_URL+"/api/todo/update",
      JSON.stringify(todoUpdateMode),
      {
        headers: 
        {
          "Content-Type": "application/json",
          "Authorization": Auth.GetAuthorizationHeader()
        },
      }
    );

    updateTodo();
  };

  const HandleLogout = () => {
    axios.post(Base.BASE_URL+"/api/auth/logout").then((x) => {
      window.location.href = "/login";
    });
  };

  const FetchTodoList = () => {
    axios
      .get(Base.BASE_URL+"/api/todo/fetch", {
        headers: 
        {
          "Content-Type": "application/json",
          "Authorization": Auth.GetAuthorizationHeader()
        },
      })
      .then((response) => setTodos(response.data));
  };

  return (
    <div className="App">
      <div className="creator">
        <h2>Todos ({todos.length})</h2>
        <input
          placeholder="What's your next step?"
          onKeyDown={OnInputEnter}
          onChange={OnInputChange}
          id="todo-input"
          type="text"
          value={inputValue}
        ></input>
        <h2>Completed: {completedCount}</h2>
      </div>
      {
        <div className="todos">
          <table>
            <tbody>
              <tr>
                <th>Title</th>
                <th className="header-todo-header-max">Date</th>
                <th className="header-todo-header-max">Progress</th>
                <th></th>
              </tr>
              {todos.map((todo, index) => {
                return (
                  <tr key={"todo_" + index}>
                    <td>
                      <div className="todo-title">
                        <span>{todo.Name}</span>
                      </div>
                    </td>
                    <td className="header-todo-header-max">
                      <div>
                        <span>{todo.Date}</span>
                      </div>
                    </td>
                    <td className="header-todo-header-max">
                      <div
                        onClick={() => HandleProgressClicked(todo)}
                        style={{
                          backgroundColor: GetProgressColor(
                            GetProgressByIndex(todo.ProgressId)
                          ),
                        }}
                        className="TodoProgress"
                      >
                        {GetProgressName(todo.ProgressId)}
                      </div>
                    </td>
                    <th>
                      <img
                        alt="delete"
                        onClick={() => HandleDelete(todo)}
                        src={require("./delete.png")}
                      ></img>
                    </th>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      }
    </div>
  );
}

export default Todo;
