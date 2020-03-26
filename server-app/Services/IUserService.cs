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
        User Create(RegisterModel registerModel);
        void ChangePassword(ChangePasswordModel changePasswordModel, int userId);
        void ChangeNamesUser(ChangeNameUserModel changeNameUserModel, int userId);
        User GetById(int id);
        void Update(User user, string password = null);
        void Delete(int id);
    }
}
