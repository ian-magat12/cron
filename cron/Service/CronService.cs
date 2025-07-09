
using cron.Models;
using System.Text.Json;

namespace cron.Service
{
    public class CronService : BackgroundService
    {
        private readonly ILogger<CronService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private List<ScheduledTask> _tasks;
        private readonly string _taskFilePath;

        public CronService(ILogger<CronService> logger, IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            // Example static task list
             _taskFilePath = Path.Combine(env.ContentRootPath, "tasks.json");
        }


        protected override async  Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _tasks = await LoadTasksAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                bool updated = false;

                foreach (var task in _tasks)
                {
                    if (DateTime.UtcNow - task.LastFired >= TimeSpan.FromMinutes(task.IntervalMinutes))
                    {
                        await FireUrl(task);
                        task.LastFired = DateTime.UtcNow;
                        updated = true;
                    }
                }

                if (updated)
                {
                    await SaveTasksAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        private async Task<List<ScheduledTask>> LoadTasksAsync()
        {
            if (!File.Exists(_taskFilePath))
                return new List<ScheduledTask>();

            var json = await File.ReadAllTextAsync(_taskFilePath);
            return JsonSerializer.Deserialize<List<ScheduledTask>>(json) ?? new List<ScheduledTask>();
        }

        private async Task SaveTasksAsync()
        {
            var json = JsonSerializer.Serialize(_tasks, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_taskFilePath, json);
        }

        private async Task FireUrl(ScheduledTask task)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(task.Url);
                _logger.LogInformation($"Fired {task.Url} at {DateTime.UtcNow}, Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error firing {task.Url}: {ex.Message}");
            }
        }
    }
}
