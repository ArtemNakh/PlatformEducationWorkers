namespace PlatformEducationWorkers.Core.AddingModels.UserResults
{
    /// <summary>
    /// Represents a user's answer to a question in the context of a quiz or assessment.
    /// </summary>
    public class UserAnswerRequest
    {
        /// <summary>
        /// Gets or sets the text of the user's answer.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this answer is the correct answer.
        /// </summary>
        public bool IsCorrectAnswer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user selected this answer.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the base64-encoded string representation of the answer's image.
        /// </summary>
        public string? PhotoAnswerBase64 { get; set; }
    }
}
