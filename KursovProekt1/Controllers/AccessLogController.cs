// Контролер за логовете - показва кой кога е влизал
using ControlPanel.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlPanel.Controllers
{
    // Само Admin и Мениджър могат да виждат логовете
    [Authorize(Roles = "Admin,Мениджър")]
    public class AccessLogController : Controller
    {
        // Връзката с базата данни
        private readonly ApplicationDbContext _базаДанни;

        // Конструктор
        public AccessLogController(ApplicationDbContext context)
        {
            _базаДанни = context;
        }

        // ── Списък с логове ───────────────────────────────────────────────────
        // status = филтър по статус (Approved/Denied/Pending)
        // search = търсене по имейл
        public IActionResult Index(string status, string search)
        {
            // Вземаме всички логове
            // AsQueryable = не изпълняваме заявката още, само я подготвяме
            var логове = _базаДанни.AccessLogs
                .Include(л => л.User)
                .Include(л => л.Room)
                .AsQueryable();

            // Ако е избран статус - филтрираме по него
            bool имаФилтърСтатус = !string.IsNullOrEmpty(status);
            if (имаФилтърСтатус)
                логове = логове.Where(л => л.Status == status);

            // Ако има търсене - филтрираме по имейл
            bool имаТърсене = !string.IsNullOrEmpty(search);
            if (имаТърсене)
                логове = логове.Where(л =>
                    л.User != null &&
                    л.User.Email != null &&
                    л.User.Email.Contains(search));

            // Пращаме избраните стойности обратно към View-то
            // За да останат в полетата след търсене
            ViewBag.SelectedStatus = status;
            ViewBag.SearchQuery = search;

            // Изпълняваме заявката и връщаме резултата
            return View(логове.OrderByDescending(л => л.EntryTime).ToList());
        }
    }
}