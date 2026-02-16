using KursovProekt1.Data;
using KursovProekt1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Za yetkite

namespace KursovProekt1.Controllers
{
    // [Authorize] = Samo vlezli potrebiteli mogat da vlqzat
    // Roles = "Admin" = Samo potrebiteli s Admin rolq
    [Authorize(Roles = "Admin")]
    public class RoomController : Controller
    {
        // Vruzka s bazata danni
        private readonly ApplicationDbContext _context;

        // Konstruktor - poluchava bazata danni
        public RoomController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Room/Index - pokazva vsichki stai
        // [AllowAnonymous] = Vseki moje da vidi (dori i bez login)
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Vzimame vsichki stai ot bazata
            var rooms = _context.Rooms.ToList();

            // Izprashtame gi kum View
            return View(rooms);
        }

        // GET: Room/Details/5 - pokazva detaili za edna staq
        public IActionResult Details(int id)
        {
            // Nameri staqta po ID
            var room = _context.Rooms.Find(id);

            // Ako ne sastostva, pokaji greshka
            if (room == null)
            {
                return NotFound();
            }

            // Izprati q kum View
            return View(room);
        }

        // GET: Room/Create - pokazva formata za suzdavane
        public IActionResult Create()
        {
            return View();
        }

        // POST: Room/Create - zapisva novata staq v bazata
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Room room)
        {
            // Proverka dali dannite sa validni
            if (ModelState.IsValid)
            {
                // Dobavi staqta v bazata
                _context.Rooms.Add(room);

                // Zapazi promenite
                _context.SaveChanges();

                // Prenasochi kum spisuka sus stai
                return RedirectToAction("Index");
            }

            // Ako ima greshki, pokaji otnovo formata
            return View(room);
        }

        // GET: Room/Edit/5 - pokazva formata za redaktirane
        public IActionResult Edit(int id)
        {
            // Nameri staqta
            var room = _context.Rooms.Find(id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Room/Edit/5 - zapisva promenite
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Room room)
        {
            // Proverka dali ID-tata suvpadat
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Aktualizirai staqta
                _context.Update(room);

                // Zapazi promenite
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(room);
        }

        // GET: Room/Delete/5 - pokazva potvurjdenie za iztrivane
        public IActionResult Delete(int id)
        {
            var room = _context.Rooms.Find(id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Room/Delete/5 - iztriva staqta
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // Nameri staqta
            var room = _context.Rooms.Find(id);

            if (room != null)
            {
                // Iztrii q
                _context.Rooms.Remove(room);

                // Zapazi promenite
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}