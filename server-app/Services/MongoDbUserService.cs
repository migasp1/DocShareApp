using DocShareApp.Entities;
using DocShareApp.Helpers;
using DocShareApp.Mapper;
using DocShareApp.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace DocShareApp.Services
{
    public class MongoDbUserService : IUserService
    {
        private readonly IMongoClient _mongoClient;
        private readonly MongoOptions _options;
        private IUserMapper _userMapper;
        private Random _random;

        public MongoDbUserService(IMongoClient mongoCliente, IOptions<MongoOptions> options, IUserMapper usermapper, Random random)
        {
            _mongoClient = mongoCliente;
            _options = options.Value;
            _userMapper = usermapper;
            _random = random;
        }

        public async Task<User> Create(RegisterModel registerModel, CancellationToken cancellationToken = default)
        {
            IAsyncCursor<User> users = await _mongoClient.GetDatabase(_options.Database).GetCollection<User>("Users").
                FindAsync(user => user.Email.Equals(registerModel.Email), new FindOptions<User, User> {Limit = 1 }, cancellationToken).ConfigureAwait(false);


            List<User> listusers = await users.ToListAsync().ConfigureAwait(false);

            if (listusers.Any())
                throw new ApplicationException($"User with email {registerModel.Email} already exists");

            GeneratePasswordHash(registerModel.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User user = _userMapper.MapRegisterModel(registerModel, passwordHash, passwordSalt);

            user.Id = _random.Next();

            await _mongoClient.GetDatabase(_options.Database).GetCollection<User>("Users").InsertOneAsync(user).ConfigureAwait(false);

            return user;
        }

        public async Task<User> Authenticate(string email, string password, CancellationToken cancellationToken = default)
        {
            IAsyncCursor<User> users = await _mongoClient.GetDatabase(_options.Database).GetCollection<User>("Users")
            .FindAsync(user => user.Email.Equals(email), new FindOptions<User, User> { Limit = 1 }, cancellationToken).ConfigureAwait(false);

            List<User> ListUsers = await users.ToListAsync().ConfigureAwait(false);

            User user = ListUsers.FirstOrDefault();

            if (user == null)
                throw new ApplicationException("User not found");

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                throw new ApplicationException("Password does not match");

            return user;
        }

        public async Task ChangeNamesUser(ChangeNameUserModel changeNameUserModel, int userId, CancellationToken cancellationToken = default)
        {
            IAsyncCursor<User> userCollection = await _mongoClient.GetDatabase(_options.Database).GetCollection<User>("Users").FindAsync(user => user.Id.Equals(userId));

            List<User> usersList = await userCollection.ToListAsync().ConfigureAwait(false);

            User user = usersList.SingleOrDefault();

            if (user == null)
                throw new ApplicationException("User not found");

            if (user.FirstName.Equals(changeNameUserModel.NewFirstName) && user.LastName.Equals(changeNameUserModel.NewLastName))
                throw new ApplicationException("First and last names are the same as before");

            var updatesList = new List<UpdateDefinition<User>>();
            UpdateDefinitionBuilder<User> updateBuilder = Builders<User>.Update;

            if (!string.IsNullOrEmpty(changeNameUserModel.NewFirstName))
                updatesList.Add(updateBuilder.Set(user => user.FirstName, changeNameUserModel.NewFirstName));

            if (!string.IsNullOrEmpty(changeNameUserModel.NewLastName))
                updatesList.Add(updateBuilder.Set(user => user.LastName, changeNameUserModel.NewLastName));

            FilterDefinition<User> filter = Builders<User>.Filter.Eq(userin => userin.Id, userId);

            UpdateResult updateResult = await _mongoClient.GetDatabase(_options.Database).GetCollection<User>("Users")
                                                          .UpdateOneAsync(filter, updateBuilder.Combine(updatesList), null, cancellationToken).ConfigureAwait(false);

            if (!updateResult.IsAcknowledged)
                throw new ApplicationException("Could not change name(s)");
        }

        public async Task ChangePassword(ChangePasswordModel changePasswordModel, int userId, CancellationToken cancellationToken = default)
        {
            if (changePasswordModel.OldPassword.Equals(changePasswordModel.NewPassword))
                throw new ApplicationException("Old password must differ from new password");

            IAsyncCursor<User> userCollection = await _mongoClient.GetDatabase(_options.Database).GetCollection<User>("Users").FindAsync(user => user.Id.Equals(userId)).ConfigureAwait(false);

            List<User> usersList = await userCollection.ToListAsync().ConfigureAwait(false);

            User user = usersList.SingleOrDefault();

            if (user == null)
                throw new ApplicationException("User not found");

            if (!VerifyPasswordHash(changePasswordModel.OldPassword, user.PasswordHash, user.PasswordSalt))
                throw new ApplicationException("Old password is incorrect");

            GeneratePasswordHash(changePasswordModel.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            FilterDefinition<User> filter = Builders<User>.Filter.Eq(user => user.Id, user.Id);
            UpdateDefinition<User> update = Builders<User>.Update.Set(user => user.PasswordHash, passwordHash)
                                                                 .Set(user => user.PasswordSalt, passwordSalt);

            UpdateResult a = await _mongoClient.GetDatabase(_options.Database).GetCollection<User>("Users").UpdateOneAsync(filter, update, null, cancellationToken);
        }

        public async Task Delete(int userId, CancellationToken cancellationToken = default)
        {
            IAsyncCursor<User> userCollection = await _mongoClient.GetDatabase(_options.Database).GetCollection<User>("Users").FindAsync(user => user.Id.Equals(userId)).ConfigureAwait(false);

            List<User> usersList = await userCollection.ToListAsync().ConfigureAwait(false);

            User user = usersList.SingleOrDefault();

            if (user == null)
                throw new ApplicationException("User not found");

            DeleteResult deleteResult = await _mongoClient.GetDatabase(_options.Database).GetCollection<User>("Users").DeleteOneAsync(user => user.Id.Equals(userId));

            if (!deleteResult.IsAcknowledged)
                throw new ApplicationException("Delete operation was not successful");
        }
        public async Task<IEnumerable<User>> GetAllUsers(CancellationToken cancellationToken = default)
        {
            List<User> usersList = await _mongoClient.GetDatabase(_options.Database).GetCollection<User>("Users").AsQueryable().ToListAsync().ConfigureAwait(false);

            return usersList;
        }

        public async Task<User> RetrievePersonalUserInfo(int id, CancellationToken cancellationToken = default)
        {
            IAsyncCursor<User> usersCollection = await _mongoClient.GetDatabase(_options.Database).GetCollection<User>("Users").FindAsync(user => user.Id.Equals(id)).ConfigureAwait(false);

            List<User> usersList = await usersCollection.ToListAsync().ConfigureAwait(false);

            User user = usersList.SingleOrDefault();

            return user;
        }

        private void GeneratePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Value cannot be empty nor contain whitespaces", "password");

            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt);
            byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            if (storedHash.Length != computedHash.Length)
                throw new ApplicationException("User or password incorrect");

            for (int i = 0; i < storedHash.Length; i++)
            {
                if (computedHash[i] != storedHash[i])
                    throw new ApplicationException("User or password incorrect");
            }
            return true;
        }
    }
}
