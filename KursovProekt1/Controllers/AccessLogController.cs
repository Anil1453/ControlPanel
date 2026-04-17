using ControlPanel.Data;
using ControlPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlPanel.Controllers
{
    // Admin ve Мениджър logları görebilir
    [Authorize(Roles = "Admin,Мениджър")]
    public class AccessLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccessLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string status)
        {
            var logs = _context.AccessLogs
                .Include(a => a.User)
                .Include(a => a.Room)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                logs = logs.Where(a => a.Status == status);

            ViewBag.SelectedStatus = status;
            return View(logs.OrderByDescending(a => a.EntryTime).ToList());
        }
    }
}
