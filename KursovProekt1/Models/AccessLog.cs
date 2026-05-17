// Лог за достъп - записваме всяко влизане и излизане
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel.Models
{
    public class AccessLog
    {
        // Уникален номер на записа
        [Key]
        public int Id { get; set; }

        // ID на потребителя
        [Required]
        public string UserId { get; set; }

        // Самият потребител (зарежда се от базата)
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        // ID на стаята
        [Required]
        public int RoomId { get; set; }

        // Самата стая (зарежда се от базата)
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        // Времето на влизане
        public DateTime EntryTime { get; set; } = DateTime.Now;

        // Времето на излизане - празно означава "все още вътре"
        public DateTime? ExitTime { get; set; }

        // Резултатът от опита за влизане:
        // "Approved" = влязъл успешно
        // "Denied"   = отказан
        // "Pending"  = чака одобрение
        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        // Допълнителни бележки (не е задължително)
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}