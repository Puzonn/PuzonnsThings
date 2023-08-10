import Cookies from "js-cookie";
import { useEffect } from "react";
import "./Styles/NavContextStyle.css";

export const NavContextComponent = ({ HandleContextExit }: any) => {
  const HandleLogout = () => {
    Cookies.remove("Bearer");
    window.addEventListener("click", () => {
      HandleContextExit();
    }, false);
    window.location.href = "/login";
  };

  return (
    <div className="nav-context-container">
      <ul>
        <li>
          <a>My Profile</a>
        </li>
        <li>
          <a>Change Language</a>
        </li>
        <li>
          <a>Terms Of Use</a>
        </li>
        <li>
          <a>Help</a>
        </li>
        <hr></hr>
        <li onClick={HandleLogout}>Logout</li>
      </ul>
    </div>
  );
};
