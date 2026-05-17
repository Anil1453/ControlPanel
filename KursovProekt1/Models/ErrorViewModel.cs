// Модел за страницата с грешки - пази ID-то на заявката
namespace ControlPanel.Models
{
    public class ErrorViewModel
    {
        // ID на заявката, при която е станала грешката
        public string? RequestId { get; set; }

        // Показваме ли ID-то? Само ако не е празно
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}