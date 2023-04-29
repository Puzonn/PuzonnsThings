import Cookies from "js-cookie";
import { useContext } from "react";
import { AuthContext } from "./AuthContext";
import { UserContext } from "./UserContext";

export const NavBar = () => {
  const { isLoggedIn } = useContext(AuthContext);
  const { Username, Points } = useContext(UserContext);

  const HandleLogout = () => {
    Cookies.remove("Bearer");
    window.location.href = "/login";
  };

  if (!isLoggedIn) {
    return <div></div>;
  }

  return (
    <header>
      <h3>Puzonne Things</h3>
      <nav>
        <ul className="nav-links">
          <li>
            <a className="nav-link" href="/todo">
              Todo
            </a>
          </li>
          <li>
            <a className="nav-link" href="/watchtogether">
              Watch Together
            </a>
          </li>
          <li>
            <a className="nav-link" href="/ludo">
              Ludo
            </a>
          </li>
          <li>
            <a className="nav-link" href="/yahtzee">
              Yahtzee
            </a>
          </li>
        </ul>
      </nav>
      <div>
        <span className="nav-item">
          {Username} {Points}
        </span>
        <button onClick={HandleLogout}>Logout</button>
      </div>
    </header>
  );
};
