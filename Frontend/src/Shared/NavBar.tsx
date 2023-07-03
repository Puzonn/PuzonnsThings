import Cookies from "js-cookie";
import { useContext, useEffect, useState } from "react";
import { AuthContext } from "./AuthContext";
import { UserContext } from "./UserContext";
import MenuIcon from "../Icons/icon_menu.svg";
import LoginIcon from "../Icons/icon_login.svg";
import LogoutIcon from "../Icons/icon_logout.svg";

export const NavBar = () => {
  const { isLoggedIn } = useContext(AuthContext);
  const { Username, Coins } = useContext(UserContext);
  const [screenWidth, setScreenWidth] = useState<number>(window.innerWidth);
  const [showMenuIcon, setShowMenuIcon] = useState<boolean>(false);

  const IsMobile = screenWidth <= 768;
  const MobileShowMenu = IsMobile && showMenuIcon;
  const ShouldShowMenu = MobileShowMenu || !IsMobile;

  const HandleLogout = () => {
    Cookies.remove("Bearer");

    window.location.href = "/login";
  };

  useEffect(() => {
    window.addEventListener(
      "resize",
      (event) => {
        setScreenWidth(window.innerWidth);
      },
      true
    );
  }, []);

  if (IsMobile && !isLoggedIn)
    return (
      <div>
        <header>
          <nav>
            {!showMenuIcon && (
              <a href="/home" id="nav-home-btn">
                PuzonnThings
              </a>
            )}
          </nav>
          <div>
            <img
              id="nav-menu-icon"
              onClick={() => setShowMenuIcon(showMenuIcon ? false : true)}
              src={MenuIcon}
            />
          </div>
        </header>
        <div
          style={{ display: showMenuIcon ? "initial" : "none" }}
          id="nav-menu-context"
        >
          <ul
            className={showMenuIcon ? "nav-menu-context-animation" : " "}
            style={{ overflow: "hidden" }}
          >
            <li>
              <a style={{ fontSize: "4vw" }} id="nav-home-btn" href="/contact">
                PuzonnThings
              </a>
            </li>
            <li>
              <a href="/contact">Contact</a>
            </li>
            <li className="nav-menu-context-grid">
              <img src={LoginIcon} />
              <a href="/login">Login</a>
            </li>
          </ul>
        </div>
      </div>
    );

  if (IsMobile && isLoggedIn)
    return (
      <div>
        <header>
          <nav>
            {!showMenuIcon && (
              <a href="/" id="nav-home-btn">
                PuzonnThings
              </a>
            )}
          </nav>
          <div>
            <img
              id="nav-menu-icon"
              onClick={() => setShowMenuIcon(showMenuIcon ? false : true)}
              src={MenuIcon}
            />
          </div>
        </header>
        <div
          style={{ display: showMenuIcon ? "initial" : "none" }}
          id="nav-menu-context"
        >
          <ul
            className={showMenuIcon ? "nav-menu-context-animation" : " "}
            style={{ overflow: "hidden" }}
          >
            <li>
              <a style={{ fontSize: "5vw" }} id="nav-home-btn" href="/">
                PuzonnThings
              </a>
            </li>
            <li>
              <a href="/contact">Contact & Informations</a>
            </li>
            <li className="nav-menu-context-grid">
              <img src={LogoutIcon} />
              <a onClick={HandleLogout} href="/login">
                Logout
              </a>
            </li>
          </ul>
        </div>
      </div>
    );

  return (
    <header>
      <nav>
        <a href="/home" id="nav-home-btn">
          PuzonnThings
        </a>
        <a href="/">Home</a>
        <a href="/contact">Contact & Informations</a>
      </nav>
      <div>
        {isLoggedIn && (
          <div>
            <span className="nav-item">
              {Username} {Coins} <span style={{color: 'var(--color-yellow)'}}>$</span>
            </span>
            <button onClick={HandleLogout}>Logout</button>
          </div>
        )}
        {!isLoggedIn && (
          <div className="nav-menu-context-grid">
            <img src={LoginIcon} />
            <a href="/login">Login</a>
          </div>
        )}
      </div>
    </header>
  );
};
