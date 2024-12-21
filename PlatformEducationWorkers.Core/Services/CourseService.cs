using Newtonsoft.Json;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Azure;
using System.Net.Http.Json;
using PlatformEducationWorkers.Core.AddingModels.Questions;


namespace PlatformEducationWorkers.Core.Services
{
    public class CourseService : ICourseService
    {
        private readonly IRepository _repository;
        private readonly IUserService _userService;
        //private readonly IJobTitleService _jobTitleService;
        private readonly IUserResultService _userResultService;

        private readonly AzureBlobCourseOperation AzureCourseOperation;
        public CourseService(IRepository repository, AzureBlobCourseOperation azureCourseOperation, IUserService userService,  IUserResultService userResultService)
        {
            _repository = repository;
            AzureCourseOperation = azureCourseOperation;
            _userService = userService;
            //_jobTitleService = jobTitleService;
            _userResultService = userResultService;
        }

        public async Task<Courses> AddCourse(Courses courses)
        {
            
            try
            {
                //додати валідацію
                if (courses == null)
                    throw new Exception("Cource is null");
                List<QuestionContext> questionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(courses.Questions);


                questionContexts =await  AzureCourseOperation.UploadFileToBlobAsync(questionContexts);

                courses.Questions = JsonConvert.SerializeObject(questionContexts);
                
                return await _repository.Add(courses);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error adding cource,  error:{ex}");
            }
        }

        public async Task DeleteCourse(int courseId)
        {
            try
            {
                //додати валідацію
                if (courseId == null)
                    throw new Exception("Cource is null");

                Courses course= _repository.GetById<Courses>(courseId).Result;
                List<QuestionContext> questionContexts= JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
             
                await AzureCourseOperation.DeleteFilesFromBlobAsync(questionContexts);

                await _repository.Delete<Courses>(courseId);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error delete cource,  error:{ex}");
            }
        }

        public async Task<IEnumerable<Courses>> GetAllCoursesEnterprise(int enterpriseId)
        {
            try
            {
                //додати валідацію
                if (enterpriseId == null)
                    throw new Exception("enterprice is null");
                List<Courses> coursesEnterprise=(await _repository.GetQuery<Courses>(u => u.Enterprise.Id == enterpriseId)).ToList();
                foreach (Courses course in coursesEnterprise)
                {
                    List<QuestionContext> questionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                    questionContexts=await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                    course.Questions = JsonConvert.SerializeObject(questionContexts);
                }
                return coursesEnterprise;
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get all cource enterprice by enterprice ,  error:{ex}");
            }
        }

        public async Task<IEnumerable<Courses>> GetAllCoursesUser(int userId)
        {
            try
            {
                //додати валідацію
                if (userId == null)
                    throw new Exception("user is null");
                User user = await _userService.GetUser(userId);
                if (user == null)
                    throw new Exception("User not found");


                // Змінити запит, щоб враховувати, що AccessRoles — це колекція, а JobTitle — одиночний елемент
                IEnumerable<Courses> courcesUser = (await _repository.GetQuery<Courses>(c => c.AccessRoles.Any(ar => ar.Id == user.JobTitle.Id)));

                foreach (Courses course in courcesUser)
                {
                    List<QuestionContext> questionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                    questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                    course.Questions = JsonConvert.SerializeObject(questionContexts);
                }

                return courcesUser.ToList();
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get all cource enterprice by user ,  error:{ex}");
            }
        }

        public async Task<Courses> GetCoursesById(int courseId)
        {
            try
            {
                //додати валідацію
                if (courseId == null)
                    throw new Exception("cource is null");

                Courses course= await _repository.GetByIdAsync<Courses>(courseId);
                List<QuestionContext> questionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                course.Questions = JsonConvert.SerializeObject(questionContexts);

                return course;
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get cource by id ,  error:{ex}");
            }
        }

        public async Task<IEnumerable<Courses>> GetCoursesByJobTitle(int jobTitleId)
        {
            try
            {
                if (jobTitleId == null)
                    throw new Exception("jobTitleId is null");

                JobTitle jobTitle = await _repository.GetByIdAsync<JobTitle>(jobTitleId); 
                IEnumerable<Courses> courcesJobTitle =await  _repository.GetQuery<Courses>(n => n.AccessRoles.Contains(jobTitle) );

                foreach (Courses course in courcesJobTitle)
                {
                    List<QuestionContext> questionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                    questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                    course.Questions = JsonConvert.SerializeObject(questionContexts);
                }

                return courcesJobTitle;

            }
            catch (Exception ex)
            {

                throw new Exception($"error, get courceByJobTitle, {ex}");
            }
        }

        


        public async Task<Courses> UpdateCourse(Courses course)
        {
            try
            {
                //додати валідацію
                if (course == null)
                    throw new Exception("cource is null");

               
                    

                Courses oldCource =await  _repository.GetById<Courses>(course.Id);
                //Видалення фото з ажур(стара версія курса
                List<QuestionContext> OldquestionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(oldCource.Questions);
                await AzureCourseOperation.DeleteFilesFromBlobAsync(OldquestionContexts);
               
                //додавання фото до курса(оновлення)
                List<QuestionContext> questionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                questionContexts = await AzureCourseOperation.UploadFileToBlobAsync(questionContexts);
                course.Questions = JsonConvert.SerializeObject(questionContexts);



                oldCource.TitleCource = course.TitleCource;
                oldCource.AccessRoles = course.AccessRoles;
                oldCource.Questions = course.Questions;
                oldCource.ContentCourse = course.ContentCourse;
                oldCource.Description = course.Description;

                return await  _repository.Update(course);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get cource by id ,  error:{ex}");
            }
        }



        public async Task<IEnumerable<Courses>> GetUncompletedCoursesForUser(int userId, int enterpriceId)
        {
            try
            {
                if (userId == null)
                    throw new Exception("UserId is null");
                if (enterpriceId == null)
                    throw new Exception("enterprice is null");

                // Асинхронно отримуємо результати користувача
                var userCourses = await _userResultService.GetAllUserResult(userId);

                // Асинхронно отримуємо всі курси для підприємства
                var allCourses = await _repository.GetQueryAsync<Courses>(u => u.Enterprise.Id == enterpriceId);

                // Асинхронно отримуємо користувача
                var user = await _userService.GetUser(userId);

                // Список для зберігання непройдених курсів
                List<Courses> uncompletedCourses = new List<Courses>();

                foreach (var course in allCourses)
                {
                    // Перевіряємо, чи курс був пройдений
                    bool courseCompleted = userCourses.Any(uc => uc.Course.Id == course.Id);

                    bool isJobTitleMatch = course.AccessRoles.Any(c => c.Id == user.JobTitle.Id);

                    // Якщо курс підходить по JobTitle і ще не пройдений
                    if (isJobTitleMatch && !courseCompleted)
                    {
                        uncompletedCourses.Add(course);
                    }
                }


                foreach (Courses course in uncompletedCourses)
                {
                    List<QuestionContext> questionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                    questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                    course.Questions = JsonConvert.SerializeObject(questionContexts);
                }

                // Повертаємо список непройдених курсів
                return await Task.FromResult(uncompletedCourses.AsEnumerable());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting uncompleted courses for user, error: {ex}");
            }
        }

        public async Task<IEnumerable<Courses>> GetNewCourses(int enterpriseId)
        {
            try
            {
                IEnumerable<Courses> cources = await _repository.GetQueryAsync<Courses>(
                    c => c.Enterprise.Id == enterpriseId);

                foreach (Courses course in cources)
                {
                    List<QuestionContext> questionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                    questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                    course.Questions = JsonConvert.SerializeObject(questionContexts);
                }

                return cources.OrderByDescending(course => course.DateCreate).Take(5);
               
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving new courses: {ex.Message}", ex);
            }
        }


       
    }
}
