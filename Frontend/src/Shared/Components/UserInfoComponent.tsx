import "./Styles/UserInfoStyle.css";
import CoinIcon from "../../Icons/BalanceIcon.png";
import ContextIcon from "../../Icons/icon_arrow.png";
import { useContext } from "react";
import { UserContext } from "../UserContext";

export const UserInfoComponent = ({ HandleContextClick }: any) => {
  const userContext = useContext(UserContext);

  return (
    <div className="nav-user_info-container">
      <div className="nav-user_info-balance">
        <p className="nav-user_info-username">{userContext.Username}</p>
        <div style={{ position: "relative" }}>
          <span className="nav-user_info-balance">
            <img className="nav-user_info-balance-icon" src={CoinIcon}></img>
            {Math.round(userContext.Balance)}
          </span>
        </div>
      </div>
      <div
        style={{ backgroundColor: userContext.Avatar }}
        className="nav-user_info-avatar"
      ></div>
      <div className="nav-user_info-context-container">
        <button
          className="nav-user_info-context-menu-active"
          type="button"
          title="Show/Hide Context"
          onClick={() => HandleContextClick()}
        >
          <img
            className="nav-user_info-context-menu-active"
            alt="context_icon"
            src={ContextIcon}
          />
        </button>
      </div>
    </div>
  );
};
