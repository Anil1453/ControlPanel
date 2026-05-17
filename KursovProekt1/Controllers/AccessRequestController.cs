// Контролер за заявките - изпращане, одобряване, отказване
using ControlPanel.Data;
using ControlPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlPanel.Controllers
{
    // Authorize = трябва да си влязъл в системата
    [Authorize]
    public class AccessRequestController : Controller
    {
        private readonly ApplicationDbContext _базаДанни;
        private readonly UserManager<ApplicationUser> _потребителМениджър;

        // Конструктор - получаваме базата и потребителския мениджър
        public AccessRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _базаДанни = context;
            _потребителМениджър = userManager;
        }

        // ── Списък със заявките ───────────────────────────────────────────────
        public async Task<IActionResult> Index(string search)
        {
            // Намираме ID-то на текущия потребител
            var моетоID = _потребителМениджър.GetUserId(User);

            List<AccessRequest> заявки;

            // Admin, Мениджър и Служител виждат ВСИЧКИ заявки
            bool виждаВсички = User.IsInRole("Admin") ||
                               User.IsInRole("Мениджър") ||
                               User.IsInRole("Служител");

            if (виждаВсички)
            {
                var заявкиЗаявка = _базаДанни.AccessRequests
                    .Include(з => з.User)
                    .Include(з => з.Room)
                    .AsQueryable();

                // Търсене по имейл ако е въведено
                bool имаТърсене = !string.IsNullOrEmpty(search);
                if (имаТърсене)
                    заявкиЗаявка = заявкиЗаявка.Where(з =>
                        з.User != null &&
                        з.User.Email != null &&
                        з.User.Email.Contains(search));

                заявки = await заявкиЗаявка
                    .OrderByDescending(з => з.RequestDate)
                    .ToListAsync();
            }
            else
            {
                // Обикновеният потребител вижда само СВОИТЕ заявки
                заявки = await _базаДанни.AccessRequests
                    .Include(з => з.User)
                    .Include(з => з.Room)
                    .Where(з => з.UserId == моетоID)
                    .OrderByDescending(з => з.RequestDate)
                    .ToListAsync();
            }

            ViewBag.SearchQuery = search;
            return View(заявки);
        }

        // ── Форма за нова заявка (GET) ────────────────────────────────────────
        public IActionResult Create()
        {
            // Пращаме списък с активните стаи за падащото меню
            ViewBag.Rooms = _базаДанни.Rooms.Where(с => с.IsActive).ToList();
            return View();
        }

        // ── Изпращане на нова заявка (POST) ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int roomId, string reason)
        {
            var моетоID = _потребителМениджър.GetUserId(User);

            // Проверяваме нивото на достъп на стаята
            var стая = await _базаДанни.Rooms.FindAsync(roomId);
            if (стая != null)
            {
                // Има ли потребителят нужната роля за тази стая?
                bool имаПравo =
                    стая.AccessLevel == 1 ||
                    (стая.AccessLevel == 2 && (User.IsInRole("Служител") || User.IsInRole("Мениджър") || User.IsInRole("Admin"))) ||
                    (стая.AccessLevel == 3 && (User.IsInRole("Мениджър") || User.IsInRole("Admin"))) ||
                    (стая.AccessLevel == 4 && User.IsInRole("Admin"));

                if (!имаПравo)
                {
                    TempData["Error"] = "Нямате необходимото ниво на достъп за тази зона!";
                    return RedirectToAction("Index");
                }
            }

            // Проверяваме дали вече има чакаща или одобрена заявка за тази стая
            bool вечеИмаЗаявка = await _базаДанни.AccessRequests.AnyAsync(з =>
                з.UserId == моетоID &&
                з.RoomId == roomId &&
                (з.Status == "Pending" || з.Status == "Approved"));

            if (вечеИмаЗаявка)
            {
                TempData["Error"] = "Вече имате чакаща или одобрена заявка за тази зона!";
                return RedirectToAction("Index");
            }

            // Създаваме новата заявка
            var новаЗаявка = new AccessRequest
            {
                UserId = моетоID,
                RoomId = roomId,
                Reason = reason,
                RequestDate = DateTime.Now,
                Status = "Pending",
                AdminResponse = "",
                ApprovedByAdminId = ""
            };

            _базаДанни.AccessRequests.Add(новаЗаявка);
            await _базаДанни.SaveChangesAsync();

            // Създаваме и лог запис за тази заявка
            var лог = new AccessLog
            {
                UserId = моетоID,
                RoomId = roomId,
                EntryTime = DateTime.Now,
                Status = "Pending"
            };

            _базаДанни.AccessLogs.Add(лог);
            await _базаДанни.SaveChangesAsync();

            TempData["Success"] = "Заявката е изпратена!";
            return RedirectToAction("Index");
        }

        // ── Одобряване на заявка (POST) ───────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin,Мениджър,Служител")]
        public async Task<IActionResult> Approve(int id)
        {
            // Намираме заявката по ID
            var заявка = await _базаДанни.AccessRequests.FindAsync(id);
            if (заявка == null)
                return NotFound();

            // Одобряваме я
            заявка.Status = "Approved";
            заявка.AdminResponse = "Одобрено";
            заявка.ApprovedByAdminId = _потребителМениджър.GetUserId(User);
            заявка.ApprovalDate = DateTime.Now;

            // Създаваме лог запис за одобрението
            var лог = new AccessLog
            {
                UserId = заявка.UserId,
                RoomId = заявка.RoomId,
                EntryTime = DateTime.Now,
                Status = "Approved"
            };

            _базаДанни.AccessLogs.Add(лог);
            await _базаДанни.SaveChangesAsync();

            TempData["Success"] = "Заявката е одобрена!";
            return RedirectToAction("Index");
        }

        // ── Отказване на заявка (POST) ────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin,Мениджър,Служител")]
        public async Task<IActionResult> Deny(int id)
        {
            var заявка = await _базаДанни.AccessRequests.FindAsync(id);
            if (заявка == null)
                return NotFound();

            // Отказваме я
            заявка.Status = "Denied";
            заявка.AdminResponse = "Отказано";
            заявка.ApprovedByAdminId = _потребителМениджър.GetUserId(User);
            заявка.ApprovalDate = DateTime.Now;

            // Създаваме лог запис за отказа
            var лог = new AccessLog
            {
                UserId = заявка.UserId,
                RoomId = заявка.RoomId,
                EntryTime = DateTime.Now,
                Status = "Denied"
            };

            _базаДанни.AccessLogs.Add(лог);
            await _базаДанни.SaveChangesAsync();

            TempData["Error"] = "Заявката е отказана.";
            return RedirectToAction("Index");
        }
    }
}