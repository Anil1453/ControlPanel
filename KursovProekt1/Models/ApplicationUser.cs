using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KursovProekt1.Models
{
    // Razshirqvame standartnia IdentityUser s dopulnitelna informaciq
    public class ApplicationUser : IdentityUser
    {
        // Pulno ime na potrebitelq (moje da e null pri registraciq)
        [StringLength(100)]
        public string? FullName { get; set; }  // NULLABLE!

        // Otdel (moje da e null)
        [StringLength(50)]
        public string? Department { get; set; }

        // Data na registraciq
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Aktiven li e potrebitelqt?
        public bool IsActive { get; set; } = true;

        // Vruzki
        public ICollection<AccessLog>? AccessLogs { get; set; }
        public ICollection<AccessRequest>? AccessRequests { get; set; }
    }
}