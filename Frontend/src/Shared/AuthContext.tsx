import { createContext } from "react";

export const AuthContext = createContext<AuthContextModel>({isLoggedIn: false, setAuth: () => {}});

type AuthContextModel =
{
    isLoggedIn: boolean;
    setAuth: (state: boolean) =>  void
}