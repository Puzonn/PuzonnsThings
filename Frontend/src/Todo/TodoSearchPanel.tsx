import { useState } from "react";
import menuIcon from "../Icons/icon_menu.svg";
import { SearchOptions, TodoSearchPanelProps } from "../Types/TodoTypes";

export const TodoSearchPanel = ({
  ShouldShowMenu,
  IsMobile,
  SearchOptionsSelected,
  SetSearchOption,
}: TodoSearchPanelProps): JSX.Element => {
  const [showPanelIcon, setShowPanelIcon] = useState(false);

  return (
    <div className="todo-helper">
      {IsMobile && (
        <div id="todo-menu-icon">
          <img
            onClick={() => setShowPanelIcon(showPanelIcon ? false : true)}
            src={menuIcon}
            alt="menu_icon"
          />
        </div>
      )}
      {ShouldShowMenu && (
        <div>
          <input placeholder="Search"></input>
          <ul>
            {SearchOptions.map((option) => {
              return (
                <li
                  onClick={() => SetSearchOption(option.SearchOptionType)}
                  className={
                    SearchOptionsSelected === option.SearchOptionType
                      ? "todo-helper-selected"
                      : ""
                  }
                  key={`option_${option.SearchOptionType}`}
                  style={{
                    color:
                      option.Color === undefined ? "" : `var(${option.Color})`,
                  }}
                >
                  {option.SearchName}
                </li>
              );
            })}
          </ul>
        </div>
      )}
    </div>
  );
};
