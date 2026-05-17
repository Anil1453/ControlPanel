// Стаята (зона) - тук пазим информацията за всяка стая
using System.ComponentModel.DataAnnotations;

namespace ControlPanel.Models
{
    public class Room
    {
        // Уникален номер на стаята - базата данни го задава автоматично (1, 2, 3...)
        [Key]
        public int Id { get; set; }

        // Името на стаята - задължително поле
        [Required(ErrorMessage = "Името на стаята е задължително")]
        [StringLength(100)]
        public string RoomName { get; set; }

        // Кодът на стаята (например "A101") - задължително поле
        [Required(ErrorMessage = "Кодът на стаята е задължителен")]
        [StringLength(20)]
        public string RoomCode { get; set; }

        // Описание на стаята - не е задължително
        [StringLength(500)]
        public string Description { get; set; }

        // Ниво на достъп - число от 1 до 4
        // 1 = Всички могат да влязат
        // 2 = Само служители и по-горе
        // 3 = Само мениджъри и по-горе
        // 4 = Само администратор
        [Range(1, 4)]
        public int AccessLevel { get; set; }

        // Активна ли е стаята? true = да, false = не
        public bool IsActive { get; set; } = true;

        // Списък с логове за тази стая (кой е влизал)
        public ICollection<AccessLog>? AccessLogs { get; set; }
    }
}