namespace PlatformEducationWorkers.Request.PassageCource
{
    public class UserAnswerRequest
    {
        public string Text { get; set; } // Текст відповіді
        public bool IsCorrectAnswer { get; set; } // Чи правильна ця відповідь
        public bool IsSelected { get; set; } // Чи вибрав користувач цю відповідь
        public string? PhotoAnswerBase64 { get; set; }
    }
}
