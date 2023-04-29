import { createContext } from "react";

export const UserContext = createContext<UserContextModel>({Username: '', Points: 0, setUser: () => {}});

export type UserContextModel =
{
    Username?: string;
    Points?: number;
    setUser: (state: UserContextModel) =>  void
}