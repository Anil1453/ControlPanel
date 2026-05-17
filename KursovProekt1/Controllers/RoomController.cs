// Контролер за стаите - добавяне, редактиране, изтриване, преглед
using ControlPanel.Data;
using ControlPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlPanel.Controllers
{
    public class RoomController : Controller
    {
        // Връзката с базата данни
        private readonly ApplicationDbContext _базаДанни;

        // Конструктор - базата данни се подава автоматично
        public RoomController(ApplicationDbContext context)
        {
            _базаДанни = context;
        }

        // ── Списък с всички стаи ─────────────────────────────────────────────
        // AllowAnonymous = всички могат да виждат, дори без вход
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Вземаме всички стаи от базата и ги пращаме към View-то
            var стаи = _базаДанни.Rooms.ToList();
            return View(стаи);
        }

        // ── Детайли за стая ───────────────────────────────────────────────────
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            // Намираме стаята по ID
            var стая = _базаДанни.Rooms.Find(id);

            // Ако стаята не съществува - връщаме грешка 404
            if (стая == null)
                return NotFound();

            // Намираме потребителите с одобрен достъп до тази стая
            var потребителиСДостъп = _базаДанни.AccessRequests
                .Include(з => з.User)
                .Where(з => з.RoomId == id && з.Status == "Approved")
                .ToList();

            ViewBag.UsersWithAccess = потребителиСДостъп;

            // Намираме кои потребители са вътре в момента
            // Логика: имат одобрена заявка + влезли са + не са излезли (ExitTime == null)
            var одобрениПотребителиID = _базаДанни.AccessRequests
                .Where(з => з.RoomId == id && з.Status == "Approved")
                .Select(з => з.UserId)
                .ToList();

            var вМомента = _базаДанни.AccessLogs
                .Include(л => л.User)
                .Where(л =>
                    л.RoomId == id &&
                    л.Status == "Approved" &&
                    л.ExitTime == null &&
                    одобрениПотребителиID.Contains(л.UserId))
                .GroupBy(л => л.UserId)
                .Select(г => г.OrderByDescending(л => л.EntryTime).First())
                .ToList();

            ViewBag.CurrentlyInside = вМомента;

            return View(стая);
        }

        // ── Форма за добавяне на стая (GET) ───────────────────────────────────
        // Само Admin и Мениджър могат да добавят стаи
        [Authorize(Roles = "Admin,Мениджър")]
        public IActionResult Create()
        {
            return View();
        }

        // ── Запазване на новата стая (POST) ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита срещу фалшиви заявки
        [Authorize(Roles = "Admin,Мениджър")]
        public IActionResult Create(Room стая)
        {
            // Проверяваме дали формата е попълнена правилно
            if (ModelState.IsValid)
            {
                _базаДанни.Rooms.Add(стая);
                _базаДанни.SaveChanges();
                TempData["Success"] = "Зоната е добавена успешно!";
                return RedirectToAction("Index");
            }

            // Ако има грешки - показваме формата отново
            return View(стая);
        }

        // ── Форма за редактиране на стая (GET) ────────────────────────────────
        [Authorize(Roles = "Admin,Мениджър")]
        public IActionResult Edit(int id)
        {
            var стая = _базаДанни.Rooms.Find(id);
            if (стая == null)
                return NotFound();

            return View(стая);
        }

        // ── Запазване на редактираната стая (POST) ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Мениджър")]
        public IActionResult Edit(int id, Room стая)
        {
            // Проверяваме дали ID-то съвпада
            if (id != стая.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _базаДанни.Update(стая);
                _базаДанни.SaveChanges();
                TempData["Success"] = "Зоната е обновена успешно!";
                return RedirectToAction("Index");
            }

            return View(стая);
        }

        // ── Страница за потвърждение на изтриване (GET) ───────────────────────
        [Authorize(Roles = "Admin,Мениджър")]
        public IActionResult Delete(int id)
        {
            var стая = _базаДанни.Rooms.Find(id);
            if (стая == null)
                return NotFound();

            return View(стая);
        }

        // ── Изтриване на стаята (POST) ────────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Мениджър")]
        public IActionResult DeleteConfirmed(int id)
        {
            var стая = _базаДанни.Rooms.Find(id);

            if (стая != null)
            {
                _базаДанни.Rooms.Remove(стая);
                _базаДанни.SaveChanges();
                TempData["Success"] = "Зоната е изтрита успешно!";
            }

            return RedirectToAction("Index");
        }
    }
}