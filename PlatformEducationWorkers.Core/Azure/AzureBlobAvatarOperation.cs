using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace PlatformEducationWorkers.Core.Azure
{
    /// <summary>
    /// Service for managing avatar images in Azure Blob Storage.
    /// </summary>
    public class AzureBlobAvatarOperation
    {
        private readonly BlobContainerClient _blobContainerProfileAvatar;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor for AzureBlobAvatarOperation.
        /// </summary>
        /// <param name="configuration">Configuration containing Azure Blob Storage settings.</param>
        public AzureBlobAvatarOperation(IConfiguration configuration)
        {
            _configuration = configuration;

            // Create a BlobServiceClient using the connection string from configuration
            var blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureBlobStorage"));

            // Get the blob container client for the specified container
            _blobContainerProfileAvatar = blobServiceClient.GetBlobContainerClient(_configuration["BlobNameContainer:imageprofilesavatar"]);

            // Create the container if it does not exist
            _blobContainerProfileAvatar.CreateIfNotExists();


            _configuration = configuration;
        }

        /// <summary>
        /// Uploads an avatar image to Azure Blob Storage.
        /// </summary>
        /// <param name="profileAvatar">Byte array representing the avatar image.</param>
        /// <returns>The URL of the uploaded avatar image.</returns>
        public async Task<string> UploadAvatarToBlobAsync(byte[] profileAvatar)
        {
            // Initialize the URL variable
            string profileAvatarUrl ="";

            if (profileAvatar != null)
            {
                // Generate a unique blob name using a GUID
                string blobName = $"{Guid.NewGuid()}.jpg";
                var blobClient = _blobContainerProfileAvatar.GetBlobClient(blobName);

                // Upload the file to the blob
                using (var stream = new MemoryStream(profileAvatar))
                {
                    await blobClient.UploadAsync(stream, true);
                }

                // Return the URL of the uploaded file
                profileAvatarUrl = blobClient.Uri.ToString();

            }

            return profileAvatarUrl;
        }

        /// <summary>
        /// Downloads an avatar image from Azure Blob Storage.
        /// </summary>
        /// <param name="photoUrl">The URL of the avatar image.</param>
        /// <returns>Byte array representing the downloaded image.</returns>
        public async Task<byte[]> UnloadAvatarFromBlobAsync(string photoUrl)
        {
            Uri blobUri = new Uri(photoUrl);// Create a URI from the photo URL
            string blobName = Path.GetFileName(blobUri.LocalPath);// Extract the blob name from the URI
            var blobClient = _blobContainerProfileAvatar.GetBlobClient(blobName);// Get the blob client

            // Download the blob content
            var response = await blobClient.DownloadAsync();
            byte[] photo;// Initialize the byte array for the photo
            using (var memoryStream = new MemoryStream())
            {
                await response.Value.Content.CopyToAsync(memoryStream);// Copy the content to a memory stream
                byte[] fileBytes = memoryStream.ToArray(); // Convert the memory stream to a byte array
               photo = fileBytes;
            }


            return photo;
        }

        /// <summary>
        /// Deletes an avatar image from Azure Blob Storage.
        /// </summary>
        /// <param name="photoUrl">The URL of the avatar image to be deleted.</param>
        public async Task DeleteAvatarFromBlobAsync(string photoUrl)
        {
            Uri blobUri = new Uri(photoUrl);// Create a URI from the photo URL
            string blobName = Path.GetFileName(blobUri.LocalPath);// Extract the blob name from the URI
            var blobClient = _blobContainerProfileAvatar.GetBlobClient(blobName);// Get the blob client

            // Check if the blob exists and delete it
            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();// Delete the blob
                Console.WriteLine($"Блоб {blobName} успішно видалено.");
            }
            else
            {
                Console.WriteLine($"Блоб {blobName} не знайдено.");
            }


        }
    }
}
