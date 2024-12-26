namespace PlatformEducationWorkers.Core.AddingModels.UserResults
{
    /// <summary>
    /// Represents a user's question in the context of a quiz or assessment.
    /// </summary>
    public class UserQuestionRequest
    {
        /// <summary>
        /// Gets or sets the text of the question.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the list of possible answers for the question.
        /// </summary>
        public List<UserAnswerRequest> Answers { get; set; } = new List<UserAnswerRequest>();

        /// <summary>
        /// Gets or sets the base64-encoded string representation of the question's image.
        /// </summary>
        public string? PhotoQuestionBase64 { get; set; }
    }
}
