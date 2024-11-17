import React from "react";

const TaskList = ({ tasks }) => {
    return (
        <div className="task-list">
            <h2>Список задач</h2>
            <ul>
                {tasks.map((task) => (
                    <li key={task.TaskId}>
                        {`Задача ${task.TaskId}: ${task.Status}`}
                        {task.Status === "Processing" && (
                            <progress max="100" value="50"></progress>
                        )}
                        {task.Status === "Completed" && (
                            <a href={`http://localhost:7147/api/ImageProcessing/download/${task.TaskId}`}>
                                Скачати
                            </a>
                        )}
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default TaskList;
