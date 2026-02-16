using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursovProekt1.Models
{
    // Заявки на потребители за достъп до стаи
    public class AccessRequest
    {
        // Уникален идентификатор
        [Key]
        public int Id { get; set; }

        // Кой потребител прави заявката? (Външен ключ)
        [Required]
        public string UserId { get; set; }

        // Навигационно свойство към потребителя
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        // За коя стая е заявката? (Външен ключ)
        [Required]
        public int RoomId { get; set; }

        // Навигационно свойство към стаята
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        // Причина за заявката
        [Required(ErrorMessage = "Причината е задължителна")]
        [StringLength(500)]
        public string Reason { get; set; }

        // Дата на заявката
        public DateTime RequestDate { get; set; } = DateTime.Now;

        // Статус (В изчакване, Одобрена, Отказана)
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        // Отговор от администратора
        [StringLength(500)]
        public string AdminResponse { get; set; }

        // Кой администратор одобри/отказа
        public string ApprovedByAdminId { get; set; }

        // Дата на одобрение/отказ
        public DateTime? ApprovalDate { get; set; }
    }
}