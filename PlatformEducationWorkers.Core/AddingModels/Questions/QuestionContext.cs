namespace PlatformEducationWorkers.Core.AddingModels.Questions
{
    /// <summary>
    /// Represents a question in the context of a quiz or assessment.
    /// </summary>
    public class QuestionContext
    {
        /// <summary>
        /// Gets or sets the text of the question.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the list of possible answers for the question.
        /// This is initialized to an empty list by default.
        /// </summary>
        public List<AnswerContext> Answers { get; set; } = new List<AnswerContext>();

        /// <summary>
        /// Gets or sets the base64-encoded string representation of the question's image.
        /// This property is optional and can be null.
        /// </summary>
        public string? PhotoQuestionBase64 { get; set; }

    }
}
