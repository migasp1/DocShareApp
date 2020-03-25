using DocShareApp.Entities;
using DocShareApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocShareApp.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        User Create(RegisterModel registerModel);
        void Update(User user, string password = null);
        void Delete(int id);
    }
}
