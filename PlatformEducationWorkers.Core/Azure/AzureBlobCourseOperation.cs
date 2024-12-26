using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using PlatformEducationWorkers.Core.AddingModels.Questions;
using PlatformEducationWorkers.Core.AddingModels.UserResults;

namespace PlatformEducationWorkers.Core.Azure
{
    /// <summary>
    /// Service for managing course-related images in Azure Blob Storage.
    /// </summary>
    public class AzureBlobCourseOperation
    {
        private readonly BlobContainerClient _blobContainerQuestions;
        private readonly BlobContainerClient _blobContainerAnswers;

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor for AzureBlobCourseOperation.
        /// </summary>
        /// <param name="configuration">Configuration containing Azure Blob Storage settings.</param> 
        public AzureBlobCourseOperation(IConfiguration configuration)
        {
            _configuration = configuration;

            // Create a BlobServiceClient using the connection string from configuration
            var blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureBlobStorage"));

            // Get the blob container client for the questions container
            _blobContainerQuestions = blobServiceClient.GetBlobContainerClient(_configuration["BlobNameContainer:imagequestions"]);

            // Create the container if it does not exist
            _blobContainerQuestions.CreateIfNotExists();

            // Get the blob container client for the answers container
            _blobContainerAnswers = blobServiceClient.GetBlobContainerClient(_configuration["BlobNameContainer:imageanswers"]);

            // Create the container if it does not exist
            _blobContainerAnswers.CreateIfNotExists();
        }

        /// <summary>
        /// Uploads question images to Azure Blob Storage.
        /// </summary>
        /// <param name="questions">List of questions containing images.</param>
        /// <returns>List of questions with updated image URLs.</returns>
        public async Task<List<QuestionContext>> UploadFileToBlobAsync(List<QuestionContext> questions)
        {
            foreach (var question in questions)
            {
                // Check if the question has an image
                if (question.PhotoQuestionBase64 != null)
                {
                    string blobName = $"{Guid.NewGuid()}.jpg";// Generate a unique blob name

                    byte[] fileBytes = Convert.FromBase64String(question.PhotoQuestionBase64);// Convert base64 string to byte array
                    var blobClient = _blobContainerQuestions.GetBlobClient(blobName);// Get the blob client

                    // Upload the file to the blob
                    using (var stream = new MemoryStream(fileBytes))
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    // Update the question's image URL
                    question.PhotoQuestionBase64 = blobClient.Uri.ToString();

                }

                // Upload images for each answer
                foreach (var answer in question.Answers)
                {
                    if (answer.PhotoAnswerBase64 != null)
                    {
                        string blobNameAnswer = $"{Guid.NewGuid()}.jpg";// Generate a unique blob name for the answer
                        byte[] fileBytesAnqwer = Convert.FromBase64String(answer.PhotoAnswerBase64);// Convert base64 string to byte array
                        var blobClientAnswer = _blobContainerAnswers.GetBlobClient(blobNameAnswer);// Get the blob client for the answer

                        // Upload the file to the blob
                        using (var stream = new MemoryStream(fileBytesAnqwer))
                        {
                            await blobClientAnswer.UploadAsync(stream, true);
                        }

                        // Update the answer's image URL
                        answer.PhotoAnswerBase64 = blobClientAnswer.Uri.ToString();
                    }
                }
            }

            return questions.ToList();
        }

        /// <summary>
        /// Downloads question images from Azure Blob Storage.
        /// </summary>
        /// <param name="questions">List of questions containing image URLs.</param>
        /// <returns>List of questions with images downloaded as base64 strings.</returns>
        public async Task<List<QuestionContext>> UnloadFileFromBlobAsync(List<QuestionContext> questions)
        {


            foreach (var question in questions)
            {
                // Check if the question has an image
                if (question.PhotoQuestionBase64 != null)
                {
                    Uri blobUri = new Uri(question.PhotoQuestionBase64); // Create a URI from the image URL
                    string blobName = Path.GetFileName(blobUri.LocalPath); // Extract the blob name from the URI
                    var blobClient = _blobContainerQuestions.GetBlobClient(blobName);// Get the blob client

                    // Download the file from the blob
                    var response = await blobClient.DownloadAsync();

                    using (var memoryStream = new MemoryStream())
                    {
                        await response.Value.Content.CopyToAsync(memoryStream);// Copy the content to a memory stream
                        byte[] fileBytes = memoryStream.ToArray();// Convert the memory stream to a byte array
                        question.PhotoQuestionBase64 = Convert.ToBase64String(fileBytes); // Convert the byte array to a base64 string
                    }

                }

                // Download images for each answer
                foreach (var answer in question.Answers)
                {
                    if (answer.PhotoAnswerBase64 != null)
                    {
                        Uri blobUriAnswer = new Uri(answer.PhotoAnswerBase64);// Create a URI from the answer image URL
                        string blobNameAnswer = Path.GetFileName(blobUriAnswer.LocalPath);// Extract the blob name from the URI
                        var blobClientAnswer = _blobContainerAnswers.GetBlobClient(blobNameAnswer);// Get the blob client for the answer

                        // Download the file from the blob
                        var responseAnswer = await blobClientAnswer.DownloadAsync();

                        using (var memoryStreamAnswer = new MemoryStream())
                        {
                            await responseAnswer.Value.Content.CopyToAsync(memoryStreamAnswer);// Copy the content to a memory stream
                            byte[] fileBytesAnswer = memoryStreamAnswer.ToArray();// Convert the memory stream to a byte array
                            // Конвертуємо у base64
                            answer.PhotoAnswerBase64 = Convert.ToBase64String(fileBytesAnswer); // Convert the byte array to a base64 string
                        }
                    }
                }
            }
            return questions.ToList();
        }


        /// <summary>
        /// Deletes question and answer images from Azure Blob Storage.
        /// </summary>
        /// <param name="questions">List of questions containing image URLs to be deleted.</param>
        public async Task DeleteFilesFromBlobAsync(List<QuestionContext> questions)
        {
            foreach (var question in questions)
            {
                // Delete the image for PhotoQuestionBase64
                if (!string.IsNullOrEmpty(question.PhotoQuestionBase64))
                {
                    try
                    {
                        Uri blobUri = new Uri(question.PhotoQuestionBase64);// Create a URI from the image URL
                        string blobName = Path.GetFileName(blobUri.LocalPath);// Extract the blob name from the URI

                        var blobClient = _blobContainerQuestions.GetBlobClient(blobName);// Get the blob client

                        // Check if the blob exists and delete it
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
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Помилка при видаленні блобу {question.PhotoQuestionBase64}: {ex.Message}");
                    }
                }

                // Delete images for PhotoAnswerBase64
                foreach (var answer in question.Answers)
                {
                    if (!string.IsNullOrEmpty(answer.PhotoAnswerBase64))
                    {
                        try
                        {
                            Uri blobUriAnswer = new Uri(answer.PhotoAnswerBase64);// Create a URI from the answer image URL
                            string blobNameAnswer = Path.GetFileName(blobUriAnswer.LocalPath); // Extract the blob name from the URI


                            var blobClientAnswer = _blobContainerAnswers.GetBlobClient(blobNameAnswer);// Get the blob client for the answer

                            // Check if the blob exists and delete it
                            if (await blobClientAnswer.ExistsAsync())
                            {
                                await blobClientAnswer.DeleteAsync();
                                Console.WriteLine($"Блоб {blobNameAnswer} успішно видалено.");
                            }
                            else
                            {
                                Console.WriteLine($"Блоб {blobNameAnswer} не знайдено.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Помилка при видаленні блобу {answer.PhotoAnswerBase64}: {ex.Message}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Uploads user question images to Azure Blob Storage.
        /// </summary>
        /// <param name="questions">List of user questions containing images.</param>
        /// <returns>List of user questions with updated image URLs.</returns>
        public async Task<List<UserQuestionRequest>> UploadFileToBlobAsync(List<UserQuestionRequest> questions)
        {
            foreach (var question in questions)
            {
                // Check if the user question has an imag
                if (question.PhotoQuestionBase64 != null)
                {

                    string blobName = $"{Guid.NewGuid()}.jpg";// Generate a unique blob name
                    byte[] fileBytes = Convert.FromBase64String(question.PhotoQuestionBase64);// Convert base64 string to byte array
                    var blobClient = _blobContainerQuestions.GetBlobClient(blobName);// Get the blob client

                    // Upload the file to the blob
                    using (var stream = new MemoryStream(fileBytes))
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    // Update the user's question image URL
                    question.PhotoQuestionBase64 = blobClient.Uri.ToString();

                }

                // Upload images for each answer in the user question
                foreach (var answer in question.Answers)
                {
                    if (answer.PhotoAnswerBase64 != null)
                    {
                        string blobNameAnswer = $"{Guid.NewGuid()}.jpg";// Generate a unique blob name for the answer
                        byte[] fileBytesAnqwer = Convert.FromBase64String(answer.PhotoAnswerBase64); // Convert base64 string to byte array
                        var blobClientAnswer = _blobContainerAnswers.GetBlobClient(blobNameAnswer);// Get the blob client for the answer

                        // Upload the file to the blob
                        using (var stream = new MemoryStream(fileBytesAnqwer))
                        {
                            await blobClientAnswer.UploadAsync(stream, true);
                        }

                        // Update the answer's image URL
                        answer.PhotoAnswerBase64 = blobClientAnswer.Uri.ToString();
                    }
                }
            }

            return questions.ToList();
        }

        /// <summary>
        /// Downloads user question images from Azure Blob Storage.
        /// </summary>
        /// <param name="questions">List of user questions containing image URLs.</param>
        /// <returns>List of user questions with images downloaded as base64 strings.</returns>
        public async Task<List<UserQuestionRequest>> UnloadFileFromBlobAsync(List<UserQuestionRequest> questions)
        {


            foreach (var question in questions)
            {
                // Check if the user question has an image
                if (question.PhotoQuestionBase64 != null)
                {
                    Uri blobUri = new Uri(question.PhotoQuestionBase64);
                    string blobName = Path.GetFileName(blobUri.LocalPath);
                    var blobClient = _blobContainerQuestions.GetBlobClient(blobName);

                    // Download the file from the blob
                    var response = await blobClient.DownloadAsync();

                    using (var memoryStream = new MemoryStream())
                    {
                        await response.Value.Content.CopyToAsync(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();
                        question.PhotoQuestionBase64 = Convert.ToBase64String(fileBytes);
                    }

                }

                // Download images for each answer in the user question
                foreach (var answer in question.Answers)
                {
                    if (answer.PhotoAnswerBase64 != null)
                    {
                        Uri blobUriAnswer = new Uri(answer.PhotoAnswerBase64);
                        string blobNameAnswer = Path.GetFileName(blobUriAnswer.LocalPath);
                        var blobClientAnswer = _blobContainerAnswers.GetBlobClient(blobNameAnswer);
                        
                        // Завантажуємо файл
                        var responseAnswer = await blobClientAnswer.DownloadAsync();

                        using (var memoryStreamAnswer = new MemoryStream())
                        {
                            await responseAnswer.Value.Content.CopyToAsync(memoryStreamAnswer);
                            byte[] fileBytesAnswer = memoryStreamAnswer.ToArray();
                            // Конвертуємо у base64
                            answer.PhotoAnswerBase64 = Convert.ToBase64String(fileBytesAnswer);
                        }
                    }
                }
            }
            return questions.ToList();
        }


        /// <summary>
        /// Deletes user question and answer images from Azure Blob Storage.
        /// </summary>
        /// <param name="questions">List of user questions containing image URLs to be deleted.</param>
        public async Task DeleteFilesFromBlobAsync(List<UserQuestionRequest> questions)
        {
            foreach (var question in questions)
            {
                // Delete the image for PhotoQuestionBase64
                if (!string.IsNullOrEmpty(question.PhotoQuestionBase64))
                {
                    try
                    {
                        // Отримуємо ім'я блобу з URL
                        Uri blobUri = new Uri(question.PhotoQuestionBase64);
                        string blobName = Path.GetFileName(blobUri.LocalPath);

                        // Отримуємо клієнт для блобу
                        var blobClient = _blobContainerQuestions.GetBlobClient(blobName);

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
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Помилка при видаленні блобу {question.PhotoQuestionBase64}: {ex.Message}");
                    }
                }

                // Видалення файлів для PhotoAnswerBase64
                foreach (var answer in question.Answers)
                {
                    if (!string.IsNullOrEmpty(answer.PhotoAnswerBase64))
                    {
                        try
                        {
                            // Отримуємо ім'я блобу з URL
                            Uri blobUriAnswer = new Uri(answer.PhotoAnswerBase64);
                            string blobNameAnswer = Path.GetFileName(blobUriAnswer.LocalPath);

                            // Отримуємо клієнт для блобу
                            var blobClientAnswer = _blobContainerAnswers.GetBlobClient(blobNameAnswer);

                            // Перевіряємо, чи існує блоб, і видаляємо його
                            if (await blobClientAnswer.ExistsAsync())
                            {
                                await blobClientAnswer.DeleteAsync();
                                Console.WriteLine($"Блоб {blobNameAnswer} успішно видалено.");
                            }
                            else
                            {
                                Console.WriteLine($"Блоб {blobNameAnswer} не знайдено.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Помилка при видаленні блобу {answer.PhotoAnswerBase64}: {ex.Message}");
                        }
                    }
                }
            }
        }

    }
}
