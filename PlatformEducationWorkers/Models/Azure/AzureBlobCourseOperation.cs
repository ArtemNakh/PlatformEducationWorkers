using Azure.Storage.Blobs;
using PlatformEducationWorkers.Models.Questions;
using PlatformEducationWorkers.Models.Results;

namespace PlatformEducationWorkers.Models.Azure
{
    public class AzureBlobCourseOperation
    {
        private readonly BlobContainerClient _blobContainerQuestions;
        private readonly BlobContainerClient _blobContainerAnswers;

        private readonly IConfiguration _configuration;
        public AzureBlobCourseOperation(IConfiguration configuration)
        {
            _configuration = configuration;
            var blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureBlobStorage"));
            _blobContainerQuestions = blobServiceClient.GetBlobContainerClient(_configuration["BlobNameContainer:imagequestions"]);
            _blobContainerQuestions.CreateIfNotExists();

            _blobContainerAnswers = blobServiceClient.GetBlobContainerClient(_configuration["BlobNameContainer:imageanswers"]);
            _blobContainerAnswers.CreateIfNotExists();
            _configuration = configuration;
        }

        public async Task<List<QuestionContext>> UploadFileToBlobAsync(List<QuestionContext> questions)
        {
            foreach (var question in questions)
            {

                if (question.PhotoQuestionBase64 != null)
                {
                    // Uri blobUri = new Uri(question.PhotoQuestionBase64);
                    string blobName = $"{Guid.NewGuid()}.jpg";
                    //  string blobName = Path.GetFileName(blobUri.LocalPath);
                    byte[] fileBytes = Convert.FromBase64String(question.PhotoQuestionBase64);
                    var blobClient = _blobContainerQuestions.GetBlobClient(blobName);

                    // Завантажуємо файл
                    using (var stream = new MemoryStream(fileBytes))
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    // Повертаємо URL завантаженого файлу
                    question.PhotoQuestionBase64 = blobClient.Uri.ToString();

                }

                foreach (var answer in question.Answers)
                {
                    if (answer.PhotoAnswerBase64 != null)
                    {
                        //Uri blobUriAnswer = new Uri(answer.PhotoAnswerBase64);
                        //string blobNameAnswer = Path.GetFileName(blobUriAnswer.LocalPath);
                        string blobNameAnswer = $"{Guid.NewGuid()}.jpg";
                        byte[] fileBytesAnqwer = Convert.FromBase64String(answer.PhotoAnswerBase64);
                        var blobClientAnswer = _blobContainerAnswers.GetBlobClient(blobNameAnswer);

                        // Завантажуємо файл
                        using (var stream = new MemoryStream(fileBytesAnqwer))
                        {
                            await blobClientAnswer.UploadAsync(stream, true);
                        }

                        // Повертаємо URL завантаженого файлу
                        answer.PhotoAnswerBase64 = blobClientAnswer.Uri.ToString();
                    }
                }
            }

            return questions.ToList();
        }
        public async Task<List<QuestionContext>> UnloadFileFromBlobAsync(List<QuestionContext> questions)
        {


            foreach (var question in questions)
            {

                if (question.PhotoQuestionBase64 != null)
                {
                    //Uri blobUri = new Uri(question.PhotoQuestionBase64);
                    //string blobName = Path.GetFileName(blobUri.LocalPath);
                    Uri blobUri = new Uri(question.PhotoQuestionBase64);
                    string blobName = Path.GetFileName(blobUri.LocalPath);
                    var blobClient = _blobContainerQuestions.GetBlobClient(blobName);
                    // Отримуємо URI файла
                    // new BlobClient();
                    // var blobClient = _blobContainerQuestions.GetBlobClient(new Uri(question.PhotoQuestionBase64));
                    // Завантажуємо файл
                    var response = await blobClient.DownloadAsync();

                    using (var memoryStream = new MemoryStream())
                    {
                        await response.Value.Content.CopyToAsync(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();
                        // Конвертуємо у base64
                        question.PhotoQuestionBase64 = Convert.ToBase64String(fileBytes);
                    }

                }

                foreach (var answer in question.Answers)
                {
                    if (answer.PhotoAnswerBase64 != null)
                    {
                        Uri blobUriAnswer = new Uri(answer.PhotoAnswerBase64);
                        string blobNameAnswer = Path.GetFileName(blobUriAnswer.LocalPath);
                        var blobClientAnswer = _blobContainerAnswers.GetBlobClient(blobNameAnswer);
                        // Отримуємо URI файла
                        // var blobClientAnswer = new BlobClient(new Uri(answer.PhotoAnswerBase64));
                        //Path.GetFileName(new Uri(answer.PhotoAnswerBase64))
                        // var blobClientAnswer = _blobContainerQuestions.GetBlobClient();

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
        public async Task DeleteFilesFromBlobAsync(List<QuestionContext> questions)
        {
            foreach (var question in questions)
            {
                // Видалення файлу для PhotoQuestionBase64
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


        public async Task<List<UserQuestionRequest>> UploadFileToBlobAsync(List<UserQuestionRequest> questions)
        {
            foreach (var question in questions)
            {

                if (question.PhotoQuestionBase64 != null)
                {
                    // Uri blobUri = new Uri(question.PhotoQuestionBase64);
                    string blobName = $"{Guid.NewGuid()}.jpg";
                    //  string blobName = Path.GetFileName(blobUri.LocalPath);
                    byte[] fileBytes = Convert.FromBase64String(question.PhotoQuestionBase64);
                    var blobClient = _blobContainerQuestions.GetBlobClient(blobName);

                    // Завантажуємо файл
                    using (var stream = new MemoryStream(fileBytes))
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    // Повертаємо URL завантаженого файлу
                    question.PhotoQuestionBase64 = blobClient.Uri.ToString();

                }

                foreach (var answer in question.Answers)
                {
                    if (answer.PhotoAnswerBase64 != null)
                    {
                        //Uri blobUriAnswer = new Uri(answer.PhotoAnswerBase64);
                        //string blobNameAnswer = Path.GetFileName(blobUriAnswer.LocalPath);
                        string blobNameAnswer = $"{Guid.NewGuid()}.jpg";
                        byte[] fileBytesAnqwer = Convert.FromBase64String(answer.PhotoAnswerBase64);
                        var blobClientAnswer = _blobContainerAnswers.GetBlobClient(blobNameAnswer);

                        // Завантажуємо файл
                        using (var stream = new MemoryStream(fileBytesAnqwer))
                        {
                            await blobClientAnswer.UploadAsync(stream, true);
                        }

                        // Повертаємо URL завантаженого файлу
                        answer.PhotoAnswerBase64 = blobClientAnswer.Uri.ToString();
                    }
                }
            }

            return questions.ToList();
        }
        public async Task<List<UserQuestionRequest>> UnloadFileFromBlobAsync(List<UserQuestionRequest> questions)
        {


            foreach (var question in questions)
            {

                if (question.PhotoQuestionBase64 != null)
                {
                    //Uri blobUri = new Uri(question.PhotoQuestionBase64);
                    //string blobName = Path.GetFileName(blobUri.LocalPath);
                    Uri blobUri = new Uri(question.PhotoQuestionBase64);
                    string blobName = Path.GetFileName(blobUri.LocalPath);
                    var blobClient = _blobContainerQuestions.GetBlobClient(blobName);
                    // Отримуємо URI файла
                    // new BlobClient();
                    // var blobClient = _blobContainerQuestions.GetBlobClient(new Uri(question.PhotoQuestionBase64));
                    // Завантажуємо файл
                    var response = await blobClient.DownloadAsync();

                    using (var memoryStream = new MemoryStream())
                    {
                        await response.Value.Content.CopyToAsync(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();
                        // Конвертуємо у base64
                        question.PhotoQuestionBase64 = Convert.ToBase64String(fileBytes);
                    }

                }

                foreach (var answer in question.Answers)
                {
                    if (answer.PhotoAnswerBase64 != null)
                    {
                        Uri blobUriAnswer = new Uri(answer.PhotoAnswerBase64);
                        string blobNameAnswer = Path.GetFileName(blobUriAnswer.LocalPath);
                        var blobClientAnswer = _blobContainerAnswers.GetBlobClient(blobNameAnswer);
                        // Отримуємо URI файла
                        // var blobClientAnswer = new BlobClient(new Uri(answer.PhotoAnswerBase64));
                        //Path.GetFileName(new Uri(answer.PhotoAnswerBase64))
                        // var blobClientAnswer = _blobContainerQuestions.GetBlobClient();

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
        public async Task DeleteFilesFromBlobAsync(List<UserQuestionRequest> questions)
        {
            foreach (var question in questions)
            {
                // Видалення файлу для PhotoQuestionBase64
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
