using Microsoft.AspNetCore.Mvc;
using MyStudio.Models;
using MyStudio.Models.DatabaseContext;
using System.Diagnostics;

namespace MyStudio.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                ViewBag.DbConnection = canConnect ? "Connected" : "Disconnected";
            }
            catch (Exception ex)
            {
                ViewBag.DbConnection = $"Error: {ex.Message}";
                _logger.LogError(ex, "Database connection check failed");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Connection checking action
        public async Task<IActionResult> CheckConnection()
        {
            try
            {
                // Try to connect to database
                var canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    ViewBag.Message = "Database connection successful!";
                    ViewBag.Status = "success";
                    _logger.LogInformation("Database connection successful");
                }
                else
                {
                    ViewBag.Message = "Cannot connect to database.";
                    ViewBag.Status = "error";
                    _logger.LogWarning("Cannot connect to database");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Database connection failed: {ex.Message}";
                ViewBag.Status = "error";
                _logger.LogError(ex, "Database connection error");
            }

            return View();
        }

        // API endpoint for AJAX checking
        [HttpGet]
        public async Task<IActionResult> CheckConnectionApi()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Database connection successful!",
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cannot connect to database.",
                        timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Database connection failed: {ex.Message}",
                    timestamp = DateTime.Now
                });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}