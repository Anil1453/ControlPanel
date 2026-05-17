// Връзката с базата данни - тук казваме кои таблици съществуват
using ControlPanel.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ControlPanel.Data
{
    // IdentityDbContext вече съдържа таблиците за потребители и роли
    // Ние добавяме само нашите таблици
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Конструктор - получава настройките (connection string и т.н.)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Таблица със стаите
        public DbSet<Room> Rooms { get; set; }

        // Таблица с логовете (кой кога е влизал)
        public DbSet<AccessLog> AccessLogs { get; set; }

        // Таблица със заявките за достъп
        public DbSet<AccessRequest> AccessRequests { get; set; }
    }
}