﻿using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Models.Questions;
using PlatformEducationWorkers.Models.Questions;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.CourceRequest
{
    public class CreateCourceRequest
    {
        [Required(ErrorMessage = "Title of the course is required.")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string TitleCource { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Course content is required.")]
        public string ContentCourse { get; set; } // JSON format for course content

        [ValidateQuestions]
        public List<QuestionContext> Questions { get; set; } = new List<QuestionContext>();

        [Required(ErrorMessage = "At least one access role must be selected.")]
        public List<int> AccessRoleIds { get; set; } // List of JobTitle IDs for access roles

        [Required(ErrorMessage = "ShowQuestions must be true or false.")]
        public bool ShowCorrectAnswers { get; set; }

        [Required(ErrorMessage = "Show answers must be true or false.")]
        public bool ShowUserAnswers { get; set; }

        [Required(ErrorMessage = "Show Context in passage must be true or false.")]
        public bool ShowContextPassage { get; set; }
    }

}
