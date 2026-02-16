using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursovProekt1.Models
{
    // Записи за достъп - кой влезе в коя стая и кога
    public class AccessLog
    {
        // Уникален идентификатор
        [Key]
        public int Id { get; set; }

        // Кой потребител? (Външен ключ)
        [Required]
        public string UserId { get; set; }

        // Навигационно свойство към потребителя
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        // Коя стая? (Външен ключ)
        [Required]
        public int RoomId { get; set; }

        // Навигационно свойство към стаята
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        // Време на влизане
        public DateTime EntryTime { get; set; } = DateTime.Now;

        // Време на излизане (null ако все още е вътре)
        public DateTime? ExitTime { get; set; }

        // Статус (Одобрен, Отказан, В изчакване)
        [Required]
        [StringLength(20)]
        public string Status { get; set; } // "Approved", "Denied", "Pending"

        // Бележки
        [StringLength(500)]
        public string Notes { get; set; }
    }
}