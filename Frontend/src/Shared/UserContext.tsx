import axios from "axios";
import { createContext } from "react";
import { Config } from "./Config";
import { Auth } from "../Auth/Auth";

export const UserContext = createContext<UserContextModel>({Username: '', Coins: 0, setUser: () => {}, fetchUpdated: () => {}});

export type UserContextModel =
{
    Username: string;
    Coins: number;
    setUser: (state: UserContextModel) => void;
    fetchUpdated: () => void;
}

export const FetchSelf = async () =>  {
    const response = axios.get(Config.GetApiUrl()+`/api/users/self`, 
    {
        'headers':
        {
            'Authorization': Auth.GetAuthorizationHeader()
        }
    })

    return (await response).data;
}