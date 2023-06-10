import {
  CreatorPriorityTypes,
  Progression,
  TodoTaskListProps,
} from "../Types/TodoTypes";

export const TodoTaskList = ({
  Tasks,
  OnTaskClicked,
  IsFetched,
}: TodoTaskListProps) => {
  const ShouldRenderEmptyNote = Tasks.length === 0 && IsFetched;
  const GetProgressByIndex = (progressId: number) => {
    return parseInt(Object.keys(Progression)[progressId]);
  };

  const GetProgressColor = (progress: Progression): string => {
    if (progress === Progression.Completed) {
      return "#00FF73";
    } else if (progress === Progression.InHold) {
      return "#FFBF00";
    } else {
      return "#DC143C";
    }
  };

  const GetPriorityColor = (priority: number) => {
    if (priority === 0) {
      return "#DC143C";
    } else if (priority === 1) {
      return "#FFBF00";
    } else {
      return "#00ff73";
    }
  };

  return (
    <div id="todo-list">
      <div>
        <h2>Tasks: </h2>
        <ul>
          {Tasks.map((task, index) => {
            return (
              <div key={`task_${index}`}>
                {task.Renderable && (
                  <div>
                    <div
                      style={{
                        backgroundColor: GetProgressColor(
                          GetProgressByIndex(task.ProgressId)
                        ),
                      }}
                      className="task-progress"
                    ></div>
                    <li onClick={() => OnTaskClicked(task)}>
                      <div
                        className={
                          "task-container " +
                          (task.Selected ? "task-container-selected" : "")
                        }
                      >
                        <p className="task-name">{task.Name}</p>
                        <span className="task-date">
                          Date Due: {task.EndDate} {task.EndTime}
                          {task.Meridian}
                          <span>
                            . Priority
                            <span
                              style={{ color: GetPriorityColor(task.Priority) }}
                            >
                              {" "}
                              {CreatorPriorityTypes[task.Priority].PriorityName}
                            </span>
                          </span>
                        </span>
                      </div>
                    </li>
                  </div>
                )}
              </div>
            );
          })}
        </ul>
        {ShouldRenderEmptyNote && (
          <div className="tasks-empty">
            <h3>Looks like you have no tasks</h3>
            <span>You can create one in Task Creator</span>
          </div>
        )}
      </div>
    </div>
  );
};
