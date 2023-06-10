import { createContext } from "react";

export const AuthContext = createContext<AuthContextModel>({isLoggedIn: false, setAuth: () => {}, isFetched: false});

type AuthContextModel =
{
    isLoggedIn: boolean;
    setAuth: (state: {}) =>  void;
    isFetched: boolean;
}