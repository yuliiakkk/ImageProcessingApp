import React, { useState } from "react";
import ImageUpload from "./components/ImageUpload";
import TaskList from "./components/TaskList";
import Login from "./components/Login"
import "./App.css";

function App() {
    const [tasks, setTasks] = useState([]);
    const [loginName, setLoginName] = useState('');

    const handleNewTask = (taskIds) => {
        setTasks((prevTasks) => [...prevTasks, ...taskIds]);
    };

    const handleLoginSubmit = (name) => {
        setLoginName(name);
    }

    return (
        <div className="App">
            {(loginName && loginName.length > 0) ? (
                <>            
                    <h1>Обробка зображень</h1>
                    <ImageUpload onUpload={handleNewTask} userName={loginName} />
                    <TaskList tasks={tasks} userName={loginName}  />
                </>) : 
            (<Login onSubmit={handleLoginSubmit} />)}

        </div>
    );
}

export default App;
