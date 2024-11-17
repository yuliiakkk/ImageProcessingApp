using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace BackendApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageProcessingController : ControllerBase
    {
        private static readonly ConcurrentDictionary<int, string> TaskStatuses = new();
        private static int TaskCounter = 0;

        [HttpPost("upload")]
        public IActionResult UploadImages([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            var uploadedFiles = files.Select(file => file.FileName).ToList();

            foreach (var file in files)
            {
                var taskId = Interlocked.Increment(ref TaskCounter);
                TaskStatuses[taskId] = "Queued";
            }

            return Ok(new { Message = "Files uploaded successfully!", UploadedFiles = uploadedFiles });
        }

        [HttpPost("process")]
        public IActionResult ProcessImages([FromBody] List<int> taskIds)
        {
            if (taskIds == null || taskIds.Count == 0)
            {
                return BadRequest("No task IDs provided.");
            }

            foreach (var taskId in taskIds)
            {
                if (TaskStatuses.ContainsKey(taskId))
                {
                    TaskStatuses[taskId] = "Processing";

                    Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        TaskStatuses[taskId] = "Completed";
                    });
                }
            }

            return Ok(new { Message = "Processing started for tasks", TaskIds = taskIds });
        }

        [HttpGet("status/{taskId}")]
        public IActionResult GetTaskStatus(int taskId)
        {
            if (TaskStatuses.TryGetValue(taskId, out var status))
            {
                return Ok(new { TaskId = taskId, Status = status });
            }

            return NotFound("Task ID not found.");
        }

        [HttpGet("tasks")]
        public IActionResult GetAllTasks()
        {
            var tasks = TaskStatuses.Select(ts => new
            {
                TaskId = ts.Key,
                Status = ts.Value
            });

            return Ok(tasks);
        }

        [HttpGet("download/{taskId}")]
        public IActionResult DownloadProcessedFile(int taskId)
        {
            if (TaskStatuses.TryGetValue(taskId, out var status) && status == "Completed")
            {
                var filePath = $"./Uploads/{taskId}_processed.png";
                var fileName = $"{taskId}_processed.png";

                return PhysicalFile(filePath, "image/png", fileName);
            }

            return NotFound("Task not completed or file not found.");
        }
    }
}
