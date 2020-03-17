using DocShareApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocShareApp.Services
{
    public interface IUserService
    {
        public abstract User Authenticate(string username, string password);
        public abstract IEnumerable<User> GetAll();
        public abstract User GetById(int id);
        public abstract User Create(User user, string password);
        void Update(User user, string password = null);
        void Delete(int id);

    }
}
