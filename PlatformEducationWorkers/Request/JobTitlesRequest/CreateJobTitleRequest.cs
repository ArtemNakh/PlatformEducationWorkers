using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.JobTitlesRequest
{
    public class CreateJobTitleRequest
    {
        [Required(ErrorMessage = "Назва посади є обов'язковою.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Назва посади має містити від 3 до 100 символів..")]
        public string Name { get; set; }
    }

}
