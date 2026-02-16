using System.ComponentModel.DataAnnotations;

namespace KursovProekt1.Models
{
    // Клас за стаи/помещения с контролиран достъп
    public class Room
    {
        // Уникален идентификатор - автоматично нараства
        [Key]
        public int Id { get; set; }

        // Име на стаята (напр. "Сървърна зала")
        [Required(ErrorMessage = "Името на стаята е задължително")]
        [StringLength(100)]
        public string RoomName { get; set; }

        // Код на стаята (напр. "A101")
        [Required(ErrorMessage = "Кодът на стаята е задължителен")]
        [StringLength(20)]
        public string RoomCode { get; set; }

        // Описание
        [StringLength(500)]
        public string Description { get; set; }

        // Ниво на достъп (1=Всички, 2=Служители, 3=Мениджъри, 4=Администратор)
        [Range(1, 4)]
        public int AccessLevel { get; set; }

        // Активна ли е стаята?
        public bool IsActive { get; set; } = true;

        // Колекция от записи за достъп до тази стая
        public ICollection<AccessLog>? AccessLogs { get; set; }
    }
}