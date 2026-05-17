// Начална страница - показва статистики и последни активности
using ControlPanel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlPanel.Controllers
{
    public class HomeController : Controller
    {
        // Връзката с базата данни
        private readonly ApplicationDbContext _базаДанни;

        // Конструктор - базата данни се подава автоматично от системата
        public HomeController(ApplicationDbContext context)
        {
            _базаДанни = context;
        }

        // ── Начална страница ──────────────────────────────────────────────────
        public IActionResult Index()
        {
            // Броим записите в таблиците и ги пращаме към View-то
            // ViewBag е като "торба" - слагаме данни, View-то ги взима
            ViewBag.БройСтаи = _базаДанни.Rooms.Count();
            ViewBag.БройПотребители = _базаДанни.Users.Count();
            ViewBag.БройЛогове = _базаДанни.AccessLogs.Count();
            ViewBag.БройЗаявки = _базаДанни.AccessRequests.Count();

            // Последните 5 лога за начална страница
            // Include = зареди и свързаните данни (потребител и стая)
            // OrderByDescending = от най-новото към най-старото
            // Take(5) = вземи само 5
            ViewBag.ПоследниЛогове = _базаДанни.AccessLogs
                .Include(лог => лог.User)
                .Include(лог => лог.Room)
                .OrderByDescending(лог => лог.EntryTime)
                .Take(5)
                .ToList();

            // Последните 5 активни стаи за начална страница
            ViewBag.ПоследниСтаи = _базаДанни.Rooms
                .Where(стая => стая.IsActive)
                .OrderBy(стая => стая.RoomName)
                .Take(5)
                .ToList();

            // Статистики за логовете (за диаграмата)
            ViewBag.ОдобрениЛогове = _базаДанни.AccessLogs.Count(л => л.Status == "Approved");
            ViewBag.ОтказаниЛогове = _базаДанни.AccessLogs.Count(л => л.Status == "Denied");
            ViewBag.ЧакащиЛогове = _базаДанни.AccessLogs.Count(л => л.Status == "Pending");

            // Статистики за заявките (за диаграмата на начална страница)
            ViewBag.ОдобрениЗаявки = _базаДанни.AccessRequests.Count(з => з.Status == "Approved");
            ViewBag.ОтказаниЗаявки = _базаДанни.AccessRequests.Count(з => з.Status == "Denied");
            ViewBag.ЧакащиЗаявки = _базаДанни.AccessRequests.Count(з => з.Status == "Pending");

            // Брой чакащи заявки за известието (червено розетче в менюто)
            ViewBag.БройЧакащи = _базаДанни.AccessRequests.Count(з => з.Status == "Pending");

            return View();
        }

        // ── Страница за поверителност ─────────────────────────────────────────
        public IActionResult Privacy()
        {
            return View();
        }

        // ── Страница "Отказан достъп" ─────────────────────────────────────────
        // Показва се когато потребител се опита да влезе в забранена страница
        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        // ── Страница с информация за ролите ──────────────────────────────────
        public IActionResult Roles()
        {
            return View();
        }
    }
}