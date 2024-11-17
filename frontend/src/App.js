import React, { useState } from "react";
import ImageUpload from "./components/ImageUpload";
import TaskList from "./components/TaskList";
import "./App.css";

function App() {
    const [tasks, setTasks] = useState([]);

    const handleNewTask = (task) => {
        setTasks((prevTasks) => [...prevTasks, task]);
    };

    return (
        <div className="App">
            <h1>Обробка зображень</h1>
            <ImageUpload onUpload={handleNewTask} />
            <TaskList tasks={tasks} />
        </div>
    );
}

export default App;
