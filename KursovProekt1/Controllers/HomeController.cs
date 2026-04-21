using ControlPanel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlPanel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TotalRooms    = _context.Rooms.Count();
            ViewBag.TotalUsers    = _context.Users.Count();
            ViewBag.TotalLogs     = _context.AccessLogs.Count();
            ViewBag.TotalRequests = _context.AccessRequests.Count();

            // последни 5 лога
            ViewBag.RecentLogs = _context.AccessLogs
                .Include(a => a.User)
                .Include(a => a.Room)
                .OrderByDescending(a => a.EntryTime)
                .Take(5)
                .ToList();

            // последни 5 зони
            ViewBag.RecentRooms = _context.Rooms
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoomName)
                .Take(5)
                .ToList();

            // лог истатискити за паста диаграма
            ViewBag.ApprovedLogs = _context.AccessLogs.Count(a => a.Status == "Approved");
            ViewBag.DeniedLogs   = _context.AccessLogs.Count(a => a.Status == "Denied");
            ViewBag.PendingLogs  = _context.AccessLogs.Count(a => a.Status == "Pending");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        public IActionResult Roles()
        {
            return View();
        }
    }
}
