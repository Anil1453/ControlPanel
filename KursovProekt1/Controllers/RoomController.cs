using ControlPanel.Data;
using ControlPanel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ControlPanel.Controllers
{
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Herkes görebilir
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_context.Rooms.ToList());
        }

        // Herkes görebilir
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var room = _context.Rooms.Find(id);
            if (room == null) return NotFound();

            var usersWithAccess = _context.AccessRequests
                .Include(r => r.User)
                .Where(r => r.RoomId == id && r.Status == "Approved")
                .ToList();

            ViewBag.UsersWithAccess = usersWithAccess;
            return View(room);
        }

        // Admin ve Мениджър ekleyebilir
        [Authorize(Roles = "Admin,Мениджър")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Мениджър")]
        public IActionResult Create(Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Rooms.Add(room);
                _context.SaveChanges();
                TempData["Success"] = "Зоната е добавена успешно!";
                return RedirectToAction("Index");
            }
            return View(room);
        }

        [Authorize(Roles = "Admin,Мениджър")]
        public IActionResult Edit(int id)
        {
            var room = _context.Rooms.Find(id);
            if (room == null) return NotFound();
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Мениджър")]
        public IActionResult Edit(int id, Room room)
        {
            if (id != room.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(room);
                _context.SaveChanges();
                TempData["Success"] = "Зоната е обновена успешно!";
                return RedirectToAction("Index");
            }
            return View(room);
        }

        // Sadece Admin silebilir
        [Authorize(Roles = "Admin,Мениджър")]
        public IActionResult Delete(int id)
        {
            var room = _context.Rooms.Find(id);
            if (room == null) return NotFound();
            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Мениджър")]
        public IActionResult DeleteConfirmed(int id)
        {
            var room = _context.Rooms.Find(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                _context.SaveChanges();
                TempData["Success"] = "Зоната е изтрита успешно!";
            }
            return RedirectToAction("Index");
        }
    }
}
