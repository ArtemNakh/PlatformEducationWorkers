using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace PlatformEducationWorkers.Core.Azure
{
    public class AzureBlobAvatarOperation
    {
        private readonly BlobContainerClient _blobContainerProfileAvatar;
        private readonly IConfiguration _configuration;
        public AzureBlobAvatarOperation(IConfiguration configuration)
        {
            _configuration = configuration;
            var blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureBlobStorage"));
            _blobContainerProfileAvatar = blobServiceClient.GetBlobContainerClient(_configuration["BlobNameContainer:imageprofilesavatar"]);
            _blobContainerProfileAvatar.CreateIfNotExists();


            _configuration = configuration;
        }

        public async Task<string> UploadAvatarToBlobAsync(byte[] profileAvatar)
        {
            string profileAvatarUrl="";
            if (profileAvatar != null)
            {
                string blobName = $"{Guid.NewGuid()}.jpg";
                var blobClient = _blobContainerProfileAvatar.GetBlobClient(blobName);

                // Завантажуємо файл
                using (var stream = new MemoryStream(profileAvatar))
                {
                    await blobClient.UploadAsync(stream, true);
                }

                // Повертаємо URL завантаженого файлу
                profileAvatarUrl = blobClient.Uri.ToString();

            }




            return profileAvatarUrl;
        }
        public async Task<byte[]> UnloadAvatarFromBlobAsync(string photoUrl)
        {
            Uri blobUri = new Uri(photoUrl);
            string blobName = Path.GetFileName(blobUri.LocalPath);
            var blobClient = _blobContainerProfileAvatar.GetBlobClient(blobName);


            var response = await blobClient.DownloadAsync();
            byte[] photo;
            using (var memoryStream = new MemoryStream())
            {
                await response.Value.Content.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();
                // Конвертуємо у base64
                photo = (fileBytes);
            }


            return photo;
        }

        public async Task DeleteAvatarFromBlobAsync(string photoUrl)
        {
            Uri blobUri = new Uri(photoUrl);
            string blobName = Path.GetFileName(blobUri.LocalPath);
            var blobClient = _blobContainerProfileAvatar.GetBlobClient(blobName);


            // Перевіряємо, чи існує блоб, і видаляємо його
            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();
                Console.WriteLine($"Блоб {blobName} успішно видалено.");
            }
            else
            {
                Console.WriteLine($"Блоб {blobName} не знайдено.");
            }


        }
    }
}
