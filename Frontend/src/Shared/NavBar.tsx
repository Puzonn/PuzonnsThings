import Cookies from "js-cookie";
import { useContext, useEffect, useState } from "react";
import { AuthContext } from "./AuthContext";
import GithubIcon from "../Icons/github-mark-white.png";
import HomeIcon from "../Icons/icon_home.png";

import { UserInfoComponent } from "./Components/UserInfoComponent";
import { NavContextComponent } from "./Components/NavContextComponent";
import { Config } from "./Config";

export const NavBar = () => {
  const { isLoggedIn } = useContext(AuthContext);
  const [screenWidth, setScreenWidth] = useState<number>(window.innerWidth);
  const [isContextVisible, setContextVisible] = useState<boolean>(false);
  const IsMobile = screenWidth <= 768;

  useEffect(() => {
    window.addEventListener(
      "resize",
      (event) => {
        setScreenWidth(window.innerWidth);
      },
      true
    );
  }, []);

  const HandleContextClick = () => {
    setContextVisible(isContextVisible ? false : true);
  };

  useEffect(() => {
    if(!Config.IsNavContextEventListenerCreated){
      Config.IsNavContextEventListenerCreated = true;
      console.warn("NavContext EventListener Created, possible config defer")
      window.addEventListener("click", (event) => {
        if((event.target as any).className !== "nav-user_info-context-menu-active"){
          setContextVisible(false)
        }
      })
    }
  })

  return (
    <header>
      <div className="nav-site_title">
        <h2>PuzonnsThings</h2>
      </div>
      <div className="nav-menu-context-container">
        <div className="nav-nav_item">
          <a href="/" style={{ position: "relative" }}>
            <img className="nav-img-icon" src={HomeIcon} /> Home
          </a>
        </div>
        <div className="nav-nav_item">
          <a
            href="https://github.com/Puzonn/PuzonnsThings"
            style={{ position: "relative" }}
          >
            <img className="nav-img-icon" src={GithubIcon} />
            Github
          </a>
        </div>
      </div>
      {isLoggedIn && (
        <UserInfoComponent HandleContextClick={HandleContextClick} />
      )}
      {isContextVisible && (
        <NavContextComponent HandleContextExit={HandleContextClick} />
      )}
    </header>
  );
};
