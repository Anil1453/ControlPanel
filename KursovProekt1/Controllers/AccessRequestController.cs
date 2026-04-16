using ControlPanel.Data;
using ControlPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlPanel.Controllers
{
    // Само влезли потребители могат да използват тези страници
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

        // Показва всички заявки
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            List<AccessRequest> requests;

            if (User.IsInRole("Admin"))
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

        // Показва формата за нова заявка
        public IActionResult Create()
        {
            ViewBag.Rooms = _context.Rooms.Where(r => r.IsActive).ToList();
            return View();
        }

        // --- ОПРОСТЕН МЕТОД ЗА СЪЗДАВАНЕ ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int roomId, string reason)
        {
            // 1. Вземи ID на потребителя
            var userId = _userManager.GetUserId(User);

            // 2. ПРОВЕРКА: Има ли вече активна или чакаща заявка?
            bool kontrol = await _context.AccessRequests.AnyAsync(r =>
                r.UserId == userId &&
                r.RoomId == roomId &&
                (r.Status == "Pending" || r.Status == "Approved"));

            // 3. Ако има, спри и покажи грешка
            if (kontrol == true)
            {
                TempData["Error"] = "Вече имате чакаща или одобрена заявка за тази стая!";
                return RedirectToAction("Index");
            }

            // 4. Ако няма, създай новата заявка
            var novaZayavka = new AccessRequest
            {
                UserId = userId,
                RoomId = roomId,
                Reason = reason,
                RequestDate = DateTime.Now,
                Status = "Pending",
                AdminResponse = "",
                ApprovedByAdminId = ""
            };

            // 5. Запази в базата данни
            _context.AccessRequests.Add(novaZayavka);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Заявката е изпратена!";
            return RedirectToAction("Index");
        }

        // Админ одобрява заявка
        [HttpPost]
        [Authorize(Roles = "Admin")]
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
            return RedirectToAction("Index");
        }

        // Админ отказва заявка
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deny(int id)
        {
            var request = await _context.AccessRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = "Denied";
            request.AdminResponse = "Отказано";
            request.ApprovedByAdminId = _userManager.GetUserId(User);
            request.ApprovalDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}