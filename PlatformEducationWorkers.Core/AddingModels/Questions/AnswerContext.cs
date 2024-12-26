namespace PlatformEducationWorkers.Core.AddingModels.Questions
{
    /// <summary>
    /// Represents an answer to a question in the context of a quiz or assessment.
    /// </summary>
    public class AnswerContext
    {
        /// <summary>
        /// Gets or sets the text of the answer.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this answer is correct.
        /// </summary>
        public bool IsCorrect { get; set; }

        /// <summary>
        /// Gets or sets the base64-encoded string representation of the answer's image.
        /// This property is optional and can be null.
        /// </summary>
        public string? PhotoAnswerBase64 { get; set; }
    }
}
