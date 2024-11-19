using System;

namespace BackendApp.Models
{
    public class ImageProcessingRecord
    {
        public int Id { get; set; } // Унікальний ідентифікатор запису
        public string UserId { get; set; } // ID користувача
        public string FileName { get; set; } // Назва файлу
        public DateTime ProcessedAt { get; set; } // Час завершення обробки
    }
}
