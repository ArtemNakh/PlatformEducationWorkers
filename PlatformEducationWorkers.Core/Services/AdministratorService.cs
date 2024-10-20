//using PlatformEducationWorkers.Core.Interfaces;
//using PlatformEducationWorkers.Core.Models;
//using System.Runtime.InteropServices;
//using System.Text.Json.Serialization;


//namespace PlatformEducationWorkers.Core.Services
//{
//    public class AdministratorService : IAdministratorService
//    {
//        private readonly IRepository _context;

//        public AdministratorService(IRepository context)
//        {
//            _context = context;
//        }

//        public async Task<Administrator> AddAdministrator(Administrator administrator)
//        {
//            try
//            {
//                if (administrator.Name.Length < 3)
//                    throw new Exception("Error  lenth name less 3 letters: ");
//                if (administrator.Surname.Length < 3) throw new Exception("Error  lenth name less 3 letters: ");
//                if (administrator.Email == null) throw new Exception("Error emaile is null: ");
//                if (administrator.Birthday == null) throw new Exception("Error birthday is null: ");
//                if (administrator.Login.Length < 5) throw new Exception("Error login less than 5 letters: ");
//                if (administrator.Password.Length < 5) throw new Exception("Error password less than 5 letters: ");
//                if (administrator.Enterprice == null) throw new Exception("Error enterprice dosn`t was enter: ");

//                await _context.Add(administrator);
//                return administrator;

//            }
//            catch (Exception ex)
//            {

//                throw new Exception("Error adding administration: ", ex);
//            }
//        }

//        public Task DeleteAdministraor(int adminId)
//        {
//            try
//            {
//                return _context.Delete<Administrator>(adminId);

//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error deleting administrator: ", ex);
//            }
//        }

//        public async Task<Administrator> GetAdministratorById(int adminId)
//        {
//            try
//            {
//                return await _context.GetById<Administrator>(adminId);
//            }
//            catch (Exception ex)
//            {

//                throw new Exception("Error find administrator");
//            }
//        }

//        public async Task<IEnumerable<Administrator>> GetAllAdministratorsEnterprice(int enterpriceId)
//        {
//            try
//            {
//                IEnumerable<Administrator> administrators = await _context.GetQuery<Administrator>(a => a.Enterprice.Id == enterpriceId);
//                return administrators;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error, find all administators in exnterprice");
//            }
//        }

//        //використання бібліотеки для авторізації
//        public Task<Administrator> Login(string login, string password)
//        {
//            throw new NotImplementedException();
//        }


//        //використання бібліотеки для авторізації
//        public Task<Administrator> Register(Administrator administrator)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<Administrator> SearchAdministrator(string name, string surname, DateTime birthday)
//        {
//            try
//            {
//                return _repository.GetQuery<User>(u => u.Surname == surname && u.Name == name && u.Birthday == birthday).Result.FirstOrDefault();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"Error search user , error:{ex}");
//            }
//        }

//        //public async Task<Administrator> SearchAdministrator(string name, string surname, DateTime birthday)
//        //{
//        //    try
//        //    {
//        //        IEnumerable<Administrator> admin= await _context.GetQuery<Administrator>(a => a.Surname == surname && a.Name == name && a.Birthday == birthday);
//        //        return admin.SingleOrDefault();
//        //    }
//        //    catch (Exception ex)
//        //    {

//        //        throw new Exception("Error, search administrator");
//        //    }
//        //}

//        public async Task<Administrator> UpdateAdministrator(Administrator administrator)
//        {
//            try
//            {
//                Administrator admin = (await _context.GetQuery<Administrator>(a => a.Name == administrator.Name && a.Surname == administrator.Surname
//         && a.Birthday == administrator.Birthday)).SingleOrDefault();

//                admin.Name = administrator.Name;
//                admin.Surname = administrator.Surname;
//                admin.Birthday = administrator.Birthday;
//                admin.Login = administrator.Login;
//                admin.Email = administrator.Email;

//               return await _context.Update(admin);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("error, update administrator");
//            }

//        }
//    }
//}
