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
                Username = registerModel.Username,
                FirstName = registerModel.FirstName,
                LastName = registerModel.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
                
            };
        }

        public User MapUserModel(UserModel userModel)
        {
            return new User
            {
                Id = userModel.Id,
                Username = userModel.UserName,
                FirstName = userModel.FirstName,
                LastName = userModel.LastName
            };         
        }



    }
}
