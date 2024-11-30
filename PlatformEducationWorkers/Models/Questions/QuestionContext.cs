namespace PlatformEducationWorkers.Models.Questions
{
    public class QuestionContext
    {
        public string Text { get; set; }
        public List<AnswerContext> Answers { get; set; } = new List<AnswerContext>();
    }
}
