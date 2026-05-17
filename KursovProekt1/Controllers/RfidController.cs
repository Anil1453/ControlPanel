// RFID контролер - проверява достъп и симулира четец
using ControlPanel.Data;
using ControlPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlPanel.Controllers
{
    // ── API за реален RFID четец ──────────────────────────────────────────────
    // Реалното устройство изпраща заявка към: /api/rfid/check?userEmail=...&roomId=...
    [Route("api/rfid")]
    [ApiController]
    public class RfidController : ControllerBase
    {
        private readonly ApplicationDbContext _базаДанни;

        public RfidController(ApplicationDbContext context)
        {
            _базаДанни = context;
        }

        // Проверява дали потребителят има достъп до стаята
        // Връща JSON: { "status": "Approved" } или { "status": "Denied" }
        [HttpGet("check")]
        public async Task<IActionResult> Check(string userEmail, int roomId)
        {
            // Намираме потребителя по имейл
            var потребител = await _базаДанни.Users
                .FirstOrDefaultAsync(п => п.Email == userEmail);

            // Ако не съществува или е деактивиран - отказваме
            bool невалиденПотребител = потребител == null || !потребител.IsActive;
            if (невалиденПотребител)
                return Ok(new { status = "Denied" });

            // Проверяваме дали има одобрена заявка за тази стая
            bool имаДостъп = await _базаДанни.AccessRequests
                .AnyAsync(з =>
                    з.UserId == потребител.Id &&
                    з.RoomId == roomId &&
                    з.Status == "Approved");

            // Записваме опита за влизане в лога
            _базаДанни.AccessLogs.Add(new AccessLog
            {
                UserId = потребител.Id,
                RoomId = roomId,
                EntryTime = DateTime.Now,
                Status = имаДостъп ? "Approved" : "Denied"
            });
            await _базаДанни.SaveChangesAsync();

            // Връщаме резултата
            return Ok(new { status = имаДостъп ? "Approved" : "Denied" });
        }
    }

    // ── Симулатор страница ────────────────────────────────────────────────────
    // Позволява тестване на RFID без реално устройство
    [Authorize(Roles = "Admin,Мениджър")]
    public class RfidSimulatorController : Controller
    {
        private readonly ApplicationDbContext _базаДанни;

        public RfidSimulatorController(ApplicationDbContext context)
        {
            _базаДанни = context;
        }

        // Показваме страницата на симулатора
        public IActionResult Index()
        {
            // Пращаме активните стаи и потребители за падащите менюта
            ViewBag.Rooms = _базаДанни.Rooms.Where(с => с.IsActive).ToList();
            ViewBag.Users = _базаДанни.Users.Where(п => п.IsActive).ToList();
            return View();
        }
    }
}