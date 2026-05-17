// Потребителят - разширяваме стандартния Identity потребител с наши полета
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ControlPanel.Models
{
    // IdentityUser вече съдържа: Email, Password, Username и т.н.
    // Ние добавяме само нашите допълнителни полета
    public class ApplicationUser : IdentityUser
    {
        // Пълното име на потребителя (не е задължително)
        [StringLength(100)]
        public string? FullName { get; set; }

        // Отделът на потребителя (не е задължително)
        [StringLength(50)]
        public string? Department { get; set; }

        // Датата на регистрация - задава се автоматично при създаване
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Активен ли е потребителят?
        // false = деактивиран, не може да влезе в системата
        public bool IsActive { get; set; } = true;

        // Името на файла на профилната снимка
        // Снимките се пазят в папка wwwroot/uploads/
        [StringLength(200)]
        public string? ProfilePicture { get; set; }

        // Списък с логове на този потребител (кога е влизал)
        public ICollection<AccessLog>? AccessLogs { get; set; }

        // Списък със заявките на този потребител
        public ICollection<AccessRequest>? AccessRequests { get; set; }
    }
}