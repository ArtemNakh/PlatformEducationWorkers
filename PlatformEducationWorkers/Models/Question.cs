namespace PlatformEducationWorkers.Models
{
    public class Question
    {
        public string Text { get; set; }
        public List<string> Answers { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; } // Тут можна зберігати правильну відповідь
    }

}
