import { useContext, useEffect } from "react";
import "./Home.css";
import { AuthContext } from "../Shared/AuthContext";

export const Home = () => {
  const { isLoggedIn, isFetched } = useContext(AuthContext);
  const ShouldShowInfo = !isLoggedIn && isFetched;

  return (
    <div id="content-lists">
      <div>
        {ShouldShowInfo && (
          <div>
            <h3 style={{ fontSize: "1.5rem", color: "var(--color-red)" }}>
              You have to be logged in to join any activity !
            </h3>
            <a
              style={{
                textDecoration: "underline",
                color: "var(--text-color)",
              }}
              href="/login"
            >
              Redirect me to login page.
            </a>
          </div>
        )}
        <div className="content-list">
          <h3>Mini Games</h3>
          <ul>
            <li>
              <a href="/lobbies?type=yahtzee">Yahtzee</a>
            </li>
            <li>
              <a href="/wheel">Wheel Of Fortune</a>
            </li>
          </ul>
        </div>
        <div className="content-list">
          <h3>Other</h3>
          <ul>
            <li>
              <a href="/lobbies?type=watchtogether">Watch Together</a>
            </li>
            <li>
              <a href="/todo">Todo App</a>
            </li>
          </ul>
        </div>
      </div>
    </div>
  );
};
