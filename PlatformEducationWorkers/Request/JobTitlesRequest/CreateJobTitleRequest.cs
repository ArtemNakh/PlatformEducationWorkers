using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.JobTitlesRequest
{
    public class CreateJobTitleRequest
    {
        [Required(ErrorMessage = "Назва посади є обов'язковою.")]
        [StringLength(100, ErrorMessage = "Назва посади не може перевищувати 100 символів.")]
        public string Name { get; set; }
    }

}
