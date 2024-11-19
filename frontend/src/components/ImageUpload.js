import React, { useState } from "react";

const ImageUpload = ({ onUpload, userName }) => {
    const [files, setFiles] = useState([]);
    const [isUploading, setIsUploading] = useState(false);
    const [uploadedTasks, setUploadedTasks] = useState([]);

    const handleFileChange = (e) => {
        setFiles(e.target.files);
    };

    const handleUpload = async () => {
        if (files.length === 0) {
            alert("Будь ласка, оберіть файли для завантаження.");
            return;
        }

        setIsUploading(true);

        const formData = new FormData();
        Array.from(files).forEach((file) => formData.append("files", file));

        try {
            const response = await fetch(`https://localhost:44387/api/ImageProcessing/upload/${userName}`, {
                method: "POST",
                body: formData,
            });

            const data = await response.json();
            setIsUploading(false);

            if (response.ok) {
                setUploadedTasks(data.taskIds);
                onUpload(data.taskIds);
            } else {
                alert(data.message || "Помилка при завантаженні файлів.");
            }
        } catch (error) {
            setIsUploading(false);
            console.error("Помилка:", error);
            alert("Не вдалося з'єднатися з сервером.");
        }
    };

    const handleProcessStart = async () => {
        if (uploadedTasks.length === 0) {
            alert("Немає завантажених задач для обробки.");
            return;
        }

        try {
            const response = await fetch(`https://localhost:44387/api/ImageProcessing/process/${userName}`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(uploadedTasks),
            });

            const data = await response.json();

            if (!response.ok) {
                alert(data.message || "Помилка при запуску обробки.");
            }
        } catch (error) {
            console.error("Помилка:", error);
            alert("Не вдалося з'єднатися з сервером.");
        }
    };

    return (
        <div className="upload-section">
            <input type="file" multiple onChange={handleFileChange} />
            <button onClick={handleUpload} disabled={isUploading}>
                {isUploading ? "Завантаження..." : "Завантажити"}
            </button>
            {uploadedTasks.length > 0 && (
                <button onClick={handleProcessStart}>
                    Почати обробку
                </button>
            )}
        </div>
    );
};

export default ImageUpload;
