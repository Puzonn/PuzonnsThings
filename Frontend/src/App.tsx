import "./index.css";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Login } from "./Auth/Login";
import Todo from "./Todo/Todo";
import { Register } from "./Auth/Register";
import { WatchTogether } from "./WachTogether/WatchTogether";
import { NavBar } from "./Shared/NavBar";
import { AuthContext } from "./Shared/AuthContext";
import { FetchSelf, UserContext, UserContextModel } from "./Shared/UserContext";
import { useEffect, useState } from "react";
import { Auth } from "./Auth/Auth";
import { Yahtzee } from "./Yahtzee/Yahtzee";
import Casino from "./Casino/Casino";
import { Home } from "./Home/Home";
import { Lobbies } from "./Lobbies/Lobbies";
import { ContactAndInformations } from "./ContactAndInformations/ContactAndInformations";

export function App() {
  const [loggedUser, setUser] = useState<UserContextModel>({
    Username: "",
    Balance: 0,
    UserId: -1,
    Avatar: "",
    setUser: () => {},
    fetchUpdated: () => {},
  });
  
  const [loggedInfo, setLoggedInfo] = useState({
    isLoggedIn: false,
    isFetched: false,
  });

  useEffect(() => {
    Auth.IsLoggedIn((state) => {
      setLoggedInfo({ isLoggedIn: state, isFetched: true });

      if (state) {
        const update = FetchSelf().then((data) => {
          setUser((user) => ({
            ...user,  
            Username: data.username,
            Coins: data.balance,
            Avatar: data.avatar,
            UserId: data.userId,
          }));
        });

        loggedUser.setUser = (state) => () => setUser(state);
        loggedUser.fetchUpdated = () => update;
        loggedUser.fetchUpdated();
      }
    });
  }, []);

  return (
    <BrowserRouter>
      <UserContext.Provider
        value={{
          Avatar: loggedUser.Avatar,
          Username: loggedUser.Username,
          Balance: loggedUser.Balance,
          UserId: loggedUser.UserId,
          setUser: loggedUser.setUser,
          fetchUpdated: loggedUser.fetchUpdated,
        }}
      >
        <AuthContext.Provider
          value={{
            isFetched: loggedInfo.isFetched,
            isLoggedIn: loggedInfo.isLoggedIn,
            setAuth: () => setLoggedInfo,
          }}
        >
          <NavBar />
          <Routes>
            <Route path="/register" element={<Register />}></Route>
            <Route path="/login/:fromRegister?" element={<Login />}></Route>
            <Route path="/" element={<Home />}></Route>
            <Route path="/todo" element={<Todo />}></Route>
            <Route path="watchtogether:id?" element={<WatchTogether />}></Route>
            <Route path="/yahtzee/:id?" element={<Yahtzee />}></Route>
            <Route path="/wheel" element={<Casino />}></Route>
            <Route path="/home" element={<Home />}></Route>
            <Route path="/lobbies/:type?" element={<Lobbies />}></Route>
            <Route path="/contact" element={<ContactAndInformations />}></Route>
            <Route
              path="/informations"
              element={<ContactAndInformations />}
            ></Route>
          </Routes>
        </AuthContext.Provider>
      </UserContext.Provider>
    </BrowserRouter>
  );
}
