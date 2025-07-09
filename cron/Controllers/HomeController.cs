using cron.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace cron.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _taskFilePath;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _taskFilePath = Path.Combine(env.ContentRootPath, "tasks.json");
        }

        public async Task<IActionResult> Index()
        {
            var tasks = new List<ScheduledTask>();
            if (System.IO.File.Exists(_taskFilePath))
            {
                var json = await System.IO.File.ReadAllTextAsync(_taskFilePath);
                tasks = JsonSerializer.Deserialize<List<ScheduledTask>>(json);
            }

            return View(tasks);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost]
        public async Task<IActionResult> Save(List<ScheduledTask> tasks)
        {
            var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(_taskFilePath, json);
            return RedirectToAction("Index");
        }
    }
}
