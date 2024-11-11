using System;
using System.Collections.Generic;
using PlatformEducationWorkers.Models.Questions;
namespace PlatformEducationWorkers.Request
{
    public class EditCourceRequest
    {
        public int Id { get; set; }
        public string TitleCource { get; set; }
        public string Description { get; set; }
        public string ContentCourse { get; set; } // JSON рядок із вмістом курсу
        public List<QuestionRequest> Questions { get; set; } = new List<QuestionRequest>();
    }
}
