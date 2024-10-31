using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Core.Services
{
    public class EnterpriceService : IEnterpriceService
    {
        private readonly IRepository _repository;
        public EnterpriceService(IRepository repository)
        {
            _repository = repository;
        }

        public Task<Enterprice> AddingEnterprice(Enterprice enterprice)
        {
            try
            {

                if(enterprice == null)
                    throw new Exception($"Error adding Enterprice,enterprice is null");
                
                if (_repository.GetQuery<Enterprice>(e => e.Title == enterprice.Title).Result.Count() > 0)
                    throw new Exception($"Error adding Enterprice, Choose other name");
                if (_repository.GetQuery<Enterprice>(e => e.Email == enterprice.Email).Result.Count() > 0)
                    throw new Exception($"Error adding Enterprice, Choose other Email");

                return _repository.Add(enterprice);

            }
            catch (Exception ex)
            {

                throw new Exception($"Error adding Enterprice, error:{ex}");
            }
        }

        public Task DeleteingEnterprice(int enterpriceId)
        {
            try
            {
                //додати валідацію
                return _repository.Delete<Enterprice>(enterpriceId);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error deleting Enterprice, error:{ex}");
            }
        }

        public Task<Enterprice> GetEnterprice(int enterpriceId)
        {
            try
            {
                //додати валідацію
                return _repository.GetById<Enterprice>(enterpriceId);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error getting Enterprice by id, error:{ex}");
            }
        }

        public Task<Enterprice> GetEnterprice(string title)
        {
            try
            {
                //додати валідацію
                return Task.FromResult(_repository.GetQuery<Enterprice>(e => e.Title == title).Result.FirstOrDefault());

            }
            catch (Exception ex)
            {

                throw new Exception($"Error get Enterprice by title, error:{ex}");
            }
        }

        public Task<Enterprice> GetEnterpriceByUser(int userId)
        {
            try
            {
                User user = _repository.GetById<User>(userId).Result;
                //додати валідацію
                return Task.FromResult( _repository.GetQuery<Enterprice>(u => u.Id== user.Enterprise.Id).Result.FirstOrDefault());
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get Enterprice by user, error:{ex}");
            }
        }

        public Task<Enterprice> UpdateEnterprice(Enterprice enterprice)
        {
            try
            {
                if (enterprice == null) throw new Exception($"Error update Enterprice, entrprice is null");
                Enterprice oldEnterprice = _repository.GetById<Enterprice>(enterprice.Id).Result;

                oldEnterprice.Title = enterprice.Title;
                oldEnterprice.Email = enterprice.Email;
                
                return _repository.Update(oldEnterprice);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error update Enterprice, error:{ex}");
            }
        }

        //todo
        public Task<Enterprice> UpdateingEnterprice(Enterprice enterprice)
        {
            
            throw new NotImplementedException();
        }
    }
}
