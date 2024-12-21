namespace PlatformEducationWorkers.Core
{
    public static class AvatarHelper
    {
        private static readonly string _defaultAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Sourses", "DefaultAvatar.png");

        public static string GetDefaultAvatar()
        {
            string defaultAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Sourses", "DefaultAvatar.png");
            byte[] defaultAvatarBytes = System.IO.File.ReadAllBytes(defaultAvatarPath);
            string base64DefaultAvatar = Convert.ToBase64String(defaultAvatarBytes);
            return $"data:image/png;base64,{base64DefaultAvatar}";
        }
    }
}




