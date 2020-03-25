using DocShareApp.Entities;
using DocShareApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocShareApp.Mapper
{
    public interface IUserMapper
    {
        User MapRegisterModel(RegisterModel registerModel, byte[] passwordHash, byte[] passwordSalt);
        User MapUserModel(UserModel userModel);
    }
}
