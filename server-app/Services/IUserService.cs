using DocShareApp.Entities;
using DocShareApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DocShareApp.Services
{
    public interface IUserService
    {
        Task<User> Authenticate(string email, string password, CancellationToken cancellationToken = default);
        Task<User> Create(RegisterModel registerModel, CancellationToken cancellationToken = default);
        Task ChangePassword(ChangePasswordModel changePasswordModel, int userId, CancellationToken cancellationToken = default);
        Task ChangeNamesUser(ChangeNameUserModel changeNameUserModel, int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetAllUsers(CancellationToken cancellationToken = default);
        Task<User> RetrievePersonalUserInfo(int id, CancellationToken cancellationToken = default);
        Task Delete(int id, CancellationToken cancellationToken = default);
    }
}
