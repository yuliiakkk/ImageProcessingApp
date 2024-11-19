import React, { useState, useEffect } from "react";

const TaskList = ({ tasks, userName }) => {
    const [taskHistory, setTaskHistory] = useState([]);
    const [taskStatuses, setTaskStatuses] = useState([]);

    useEffect(() => {
        const intervalId = setInterval(async () => {
            try {
                const response = await fetch(`https://localhost:44387/api/ImageProcessing/tasks/${userName}`);
                const data = await response.json();
                setTaskStatuses(data);
            } catch (error) {
                console.error("Помилка:", error);
            }
        }, 2000);

        fetch(`https://localhost:44387/api/ImageProcessing/history/${userName}`).then(async (resp) => {
            const data = await resp.json();
            setTaskHistory(data);
        }).catch(e => console.error("Помилка:", e));

        return () => clearInterval(intervalId);
    }, []);

    return (
        <div className="task-list">
            <h2>Список задач</h2>
            <ul>
                {taskStatuses.map((task) => (
                    <li key={task.taskId}>
                        {`Задача ${task.taskId}: ${task.status}  `}
                        {task.status === "Processing" && <progress max="100" value="50"></progress>}
                        {task.status === "Completed" && (
                            <div style={{display: 'flex', flexDirection: 'column', gap: '10px'}}>
                                <a
                                    href={`https://localhost:44387/api/ImageProcessing/download/${task.taskId}/${userName}`}
                                    download
                                >
                                    Скачати
                                </a>
                                <img style={{width: '100px', height: '100px'}} src={`https://localhost:44387/api/ImageProcessing/download/${task.taskId}/${userName}`} />
                            </div>
                        )}
                        {task.status === "Error" && (
                            <span style={{ color: "red" }}>Помилка обробки</span>
                        )}
                    </li>
                ))}
            </ul>
            <h2>Історія файлів</h2>
            <ul>
                {taskHistory.map((history) => (
                    <li key={history.processedAt}>
                        {`Файл: ${history.fileName} | Оброблено: ${history.processedAt}  `}
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default TaskList;
