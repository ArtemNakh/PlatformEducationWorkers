using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.AccountRequest
{
    public class UpdateUserCredentialsRequest
    {

        [MinLength(6, ErrorMessage = "Login must be at least 6 characters long.")]
        public string? NewLogin { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? NewPassword { get; set; }


        public IFormFile? ProfileAvatar { get; set; }
    }
}

