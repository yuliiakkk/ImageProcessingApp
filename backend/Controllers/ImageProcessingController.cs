using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using BackendApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageProcessingController : ControllerBase
    {
        private static class TaskStatus
        {
            public static string Queued = "Queued";
            public static string Processing = "Processing";
            public static string Completed = "Completed";
            public static string Error = "Error";
        }

        private IDbContextFactory<AppDbContext> _dbContextFactory;
        private static readonly ConcurrentDictionary<Guid, string> UserTasks = new();
        private static readonly ConcurrentDictionary<Guid, string> TaskStatuses = new();

        public ImageProcessingController(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        [HttpPost("upload/{userName}")]
        public IActionResult UploadImages([FromForm] List<IFormFile> files, string userName)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "No files uploaded." });
            }

            var taskIds = new List<Guid>();

            foreach (var file in files)
            {
                var taskId = Guid.NewGuid();
                UserTasks[taskId] = userName;
                TaskStatuses[taskId] = TaskStatus.Queued;

                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var filePath = Path.Combine(uploadsPath, $"{taskId}_{file.FileName}");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                taskIds.Add(taskId);
            }

            return Ok(new { message = "Files uploaded successfully!", taskIds });
        }

        [HttpPost("process/{userName}")]
        public IActionResult ProcessImages([FromBody] List<string> taskIds, string userName)
        {
            if (taskIds == null || taskIds.Count == 0)
            {
                return BadRequest(new { message = "No task IDs provided." });
            }

            foreach (var taskIdString in taskIds)
            {
                var taskId = Guid.Parse(taskIdString);
                if (TaskStatuses.ContainsKey(taskId))
                {
                    TaskStatuses[taskId] = TaskStatus.Processing;

                    Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(3000); 

                            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                            var processedPath = Path.Combine(Directory.GetCurrentDirectory(), "Processed");

                            if (!Directory.Exists(processedPath))
                            {
                                Directory.CreateDirectory(processedPath);
                            }

                            var inputFile = Directory.GetFiles(uploadsPath, $"{taskId}_*").FirstOrDefault();
                            if (inputFile != null)
                            {
                                var outputFile = Path.Combine(processedPath, $"{taskId}_processed.png");

                                using (var image = SixLabors.ImageSharp.Image.Load(inputFile))
                                {
                                    image.Mutate(x => x.GaussianBlur(10));
                                    image.Save(outputFile);
                                }

                                // Логіка збереження до бази даних
                                try
                                {
                                    using var context = _dbContextFactory.CreateDbContext();
                                    var userId = userName;

                                    var record = new ImageProcessingRecord
                                    {
                                        UserId = userId,
                                        FileName = Path.GetFileName(inputFile.Split('_').Last()),
                                        ProcessedAt = DateTime.UtcNow
                                    };
                                    await context.ImageProcessingRecords.AddAsync(record);
                                    await context.SaveChangesAsync();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Database error: {ex.Message}");
                                    TaskStatuses[taskId] = TaskStatus.Error;
                                    return;
                                }
                            }

                            TaskStatuses[taskId] = TaskStatus.Completed;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Processing error: {ex.Message}");
                            TaskStatuses[taskId] = TaskStatus.Error;
                        }
                    });
                }
            }

            return Ok(new { message = "Processing started for tasks", taskIds });
        }

        [HttpGet("tasks/{userName}")]
        public IActionResult GetAllTasks(string userName)
        {
            var tasks = TaskStatuses.Where(ts => UserTasks[ts.Key] == userName).Select(ts => new
            {
                taskId = ts.Key,
                status = ts.Value
            });

            return Ok(tasks);
        }

        [HttpGet("history/{userName}")]
        public IActionResult GetHistory(string userName)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var tasks = context.ImageProcessingRecords.Where(r => r.UserId == userName).Select(r => new { r.FileName, ProcessedAt = r.ProcessedAt.ToLongTimeString() + ' ' + r.ProcessedAt.ToShortDateString() }).ToList();

            return Ok(tasks);
        }

        [HttpGet("download/{taskId}/{userName}")]
        public IActionResult DownloadProcessedFile(string taskId, string userName)
        {
            if (TaskStatuses.TryGetValue(Guid.Parse(taskId), out var status) && status == TaskStatus.Completed && UserTasks[Guid.Parse(taskId)] == userName)
            {
                var processedPath = Path.Combine(Directory.GetCurrentDirectory(), "Processed");
                var filePath = Directory.GetFiles(processedPath, $"{taskId}_*").FirstOrDefault();

                if (filePath != null)
                {
                    var fileName = Path.GetFileName(filePath);
                    var mimeType = "image/png";
                    return PhysicalFile(filePath, mimeType, fileName);
                }
            }

            return NotFound(new { message = "Task not completed or file not found." });
        }
    }
}
