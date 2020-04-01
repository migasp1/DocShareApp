using DocShareApp.Entities;
using DocShareApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocShareApp.Mapper
{
    public class UserMapper : IUserMapper
    {
        public User MapRegisterModel(RegisterModel registerModel, byte[] passwordHash, byte[] passwordSalt)
        {
            return new User
            {
                Email = registerModel.Email,
                FirstName = registerModel.FirstName,
                LastName = registerModel.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = Role.User
            };
        }

        public User MapUserModel(UserModel userModel)
        {
            return new User
            {
                Id = userModel.Id,
                Email = userModel.Email,
                FirstName = userModel.FirstName,
                LastName = userModel.LastName
            };         
        }
    }
}
