// Заявка за достъп - когато потребител иска да влезе в дадена стая
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel.Models
{
    public class AccessRequest
    {
        // Уникален номер на заявката
        [Key]
        public int Id { get; set; }

        // ID на потребителя, който прави заявката
        [Required]
        public string UserId { get; set; }

        // Самият потребител (зарежда се автоматично от базата)
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        // ID на стаята, за която е заявката
        [Required]
        public int RoomId { get; set; }

        // Самата стая (зарежда се автоматично от базата)
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        // Причината защо потребителят иска достъп - задължително
        [Required(ErrorMessage = "Причината е задължителна")]
        [StringLength(500)]
        public string Reason { get; set; }

        // Датата на заявката - задава се автоматично
        public DateTime RequestDate { get; set; } = DateTime.Now;

        // Статусът на заявката:
        // "Pending"  = чака одобрение
        // "Approved" = одобрена
        // "Denied"   = отказана
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        // Отговорът на администратора (например "Одобрено" или "Отказано")
        [StringLength(500)]
        public string AdminResponse { get; set; }

        // ID на администратора, който е одобрил/отказал
        public string ApprovedByAdminId { get; set; }

        // Датата на одобрение/отказ (може да е празна)
        public DateTime? ApprovalDate { get; set; }
    }
}