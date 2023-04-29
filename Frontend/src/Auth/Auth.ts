import axios from "axios";
import Cookies from "js-cookie";
import { Base } from "../Shared/Config";

export class Auth
{
  static GetAuthorizationHeader = () => "Bearer "+Cookies.get('Bearer');
  static GetAuthorizationToken = () => Cookies.get("Bearer") as string;

  static IsLoggedIn(callback: (isValid:boolean, tokenId: string) => void) {
    const authCookie = this.GetAuthorizationHeader();
    if(typeof authCookie === 'undefined'){
      return callback(false, '');
    }

    axios.get(Base.BASE_URL+"/api/auth/validate", 
    {
      headers:{'Authorization': this.GetAuthorizationHeader()}
    }).then(response => 
        {
          callback(response.status === 200, response.data)
        }
    ).catch(x=> {callback(false, '')})
  }

  static IsLoggedInWithRedirect(redirectUrl:string) {
    this.IsLoggedIn((response:boolean, token:string) => {
      if(response){
        window.location.href = redirectUrl;
      }
    })
  }
}