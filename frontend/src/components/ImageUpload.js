import React, { useState } from "react";

const ImageUpload = ({ onUpload }) => {
    const [files, setFiles] = useState([]);
    const [isUploading, setIsUploading] = useState(false);

    const handleFileChange = (e) => {
        setFiles(e.target.files);
    };

    const handleUpload = async () => {
        setIsUploading(true);

        const formData = new FormData();
        Array.from(files).forEach((file) => formData.append("files", file));

        const response = await fetch("http://localhost:7147/api/ImageProcessing/upload", {
            method: "POST",
            body: formData,
        });

        const data = await response.json();
        setIsUploading(false);
        onUpload(data.UploadedFiles);
    };

    return (
        <div>
            <input type="file" multiple onChange={handleFileChange} />
            <button onClick={handleUpload} disabled={isUploading}>
                {isUploading ? "Завантаження..." : "Завантажити"}
            </button>
        </div>
    );
};

export default ImageUpload;
