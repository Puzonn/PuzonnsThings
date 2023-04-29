import "./index.css";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Login } from "./Auth/Login";
import Todo from "./Todo/Todo";
import { Register } from "./Auth/Register";
import { WatchTogether } from "./WachTogether/WatchTogether";
import { NavBar } from "./Shared/NavBar";
import { AuthContext } from "./Shared/AuthContext";
import { UserContext, UserContextModel } from "./Shared/UserContext";
import { useEffect, useState } from "react";
import { Auth } from "./Auth/Auth";
import { Yahtzee } from "./Yahtzee/Yahtzee";
import { Ludo } from "./Ludo/Ludo";
import Cookies from "js-cookie";

export function App() {
  const [isLoggedIn, setLoggedIn] = useState(false);
  const [loggedUser, setUser] = useState<UserContextModel>();

  useEffect(() => {
    Auth.IsLoggedIn((state) => {
      setLoggedIn(state);
      if (state) {
        const username: string = Cookies.get("Username") as string;
        const user: UserContextModel = {
          Username: username,
          Points: 0,
          setUser: setUser,
        };
        setUser(user);
      }
    });
  }, []);

  return (
    <BrowserRouter>
      <UserContext.Provider
        value={{ Username: loggedUser?.Username, Points: loggedUser?.Points, setUser: setUser }}
      >
        <AuthContext.Provider
          value={{ isLoggedIn: isLoggedIn, setAuth: setLoggedIn }}
        >
          <NavBar />
        </AuthContext.Provider>
        <Routes>
          <Route path="/register" element={<Register />}></Route>
          <Route path="/login/:fromRegister?" element={<Login />}></Route>
          <Route path="/" element={<Login />}></Route>
          <Route path="/todo" element={<Todo />}></Route>
          <Route path="/watchtogether" element={<WatchTogether />}></Route>
          <Route path="/yahtzee/:id?" element={<Yahtzee />}></Route>
          <Route path="/ludo" element={<Ludo />}></Route>
        </Routes>
      </UserContext.Provider>
    </BrowserRouter>
  );
}
