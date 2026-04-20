using ControlPanel.Data;
using ControlPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlPanel.Controllers
{
    [Authorize]
    public class AccessRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccessRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            List<AccessRequest> requests;

            // Admin ve Мениджър ve Служител hepsini görür
            if (User.IsInRole("Admin") || User.IsInRole("Мениджър") || User.IsInRole("Служител"))
            {
                requests = await _context.AccessRequests
                    .Include(r => r.User).Include(r => r.Room)
                    .OrderByDescending(r => r.RequestDate).ToListAsync();
            }
            else
            {
                requests = await _context.AccessRequests
                    .Include(r => r.User).Include(r => r.Room)
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.RequestDate).ToListAsync();
            }
            return View(requests);
        }

        public IActionResult Create()
        {
            ViewBag.Rooms = _context.Rooms.Where(r => r.IsActive).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int roomId, string reason)
        {
            var userId = _userManager.GetUserId(User);

            bool kontrol = await _context.AccessRequests.AnyAsync(r =>
                r.UserId == userId && r.RoomId == roomId &&
                (r.Status == "Pending" || r.Status == "Approved"));

            if (kontrol)
            {
                TempData["Error"] = "Вече имате чакаща или одобрена заявка за тази зона!";
                return RedirectToAction("Index");
            }

            var novaZayavka = new AccessRequest
            {
                UserId = userId, RoomId = roomId, Reason = reason,
                RequestDate = DateTime.Now, Status = "Pending",
                AdminResponse = "", ApprovedByAdminId = ""
            };

            _context.AccessRequests.Add(novaZayavka);
            await _context.SaveChangesAsync();

            // Pending log създай
            var log = new AccessLog { UserId = userId, RoomId = roomId, EntryTime = DateTime.Now, Status = "Pending" };
            _context.AccessLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Заявката е изпратена!";
            return RedirectToAction("Index");
        }

        // Служител, Мениджър и Admin може да одобрят
        [HttpPost]
        [Authorize(Roles = "Admin,Мениджър,Служител")]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.AccessRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = "Approved";
            request.AdminResponse = "Одобрено";
            request.ApprovedByAdminId = _userManager.GetUserId(User);
            request.ApprovalDate = DateTime.Now;

            var log = new AccessLog { UserId = request.UserId, RoomId = request.RoomId, EntryTime = DateTime.Now, Status = "Approved" };
            _context.AccessLogs.Add(log);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Заявката е одобрена!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Мениджър,Служител")]
        public async Task<IActionResult> Deny(int id)
        {
            var request = await _context.AccessRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = "Denied";
            request.AdminResponse = "Отказано";
            request.ApprovedByAdminId = _userManager.GetUserId(User);
            request.ApprovalDate = DateTime.Now;

            var log = new AccessLog { UserId = request.UserId, RoomId = request.RoomId, EntryTime = DateTime.Now, Status = "Denied" };
            _context.AccessLogs.Add(log);

            await _context.SaveChangesAsync();
            TempData["Error"] = "Заявката е отказана.";
            return RedirectToAction("Index");
        }
    }
}
