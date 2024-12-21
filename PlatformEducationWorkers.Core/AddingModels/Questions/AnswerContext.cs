namespace PlatformEducationWorkers.Core.AddingModels.Questions
{
    public class AnswerContext
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public string? PhotoAnswerBase64 { get; set; }
    }
}
