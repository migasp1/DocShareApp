using DocShareApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocShareApp.Helpers;
using DocShareApp.Models;
using DocShareApp.Mapper;

namespace DocShareApp.Services
{
    public class UserService : IUserService
    {

        private DataContext _context;
        private IUserMapper _mapper;

        public UserService(DataContext context, IUserMapper userMapper)
        {
            _context = context;
            _mapper = userMapper;

        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            //Now we access the users through DbSet class implemented in DataContext
            //Culture is a language-english for example. 
            //We can apply LINQ queries to every IEnumerable! IQueryble implements IEnumerable!
            //var user = _context.Users.Where(x => x.Username.StartsWith("m", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Username);
            //var user1 = _context.Users.Where(x => x.Username.StartsWith("m")).Select(x => x.Username);
            var user = _context.Users.SingleOrDefault(x => x.Username == username);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }

        public User Create(RegisterModel registerModel)
        {
            if (string.IsNullOrEmpty(registerModel.Password))
                throw new ArgumentException("Password is required");
            if (_context.Users.Any(x => x.Username == registerModel.Username))
                throw new ApplicationException("Username \"" + registerModel.Username + "\" is already taken");

            CreatePasswordHash(registerModel.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = _mapper.MapRegisterModel(registerModel, passwordHash, passwordSalt);

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public void Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
                _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return null;
            return user;
        }

        public void Update(User userParam, string password = null)
        {
            var user = _context.Users.Find(userParam.Id);

            if (user == null)
                throw new ApplicationException("User not found");

            //update username if it has changed
            //we suppose a username already exists
            if (!string.IsNullOrEmpty(userParam.Username) && userParam.Username != user.Username)
            {
                //throw an exception if username already exists
                if (_context.Users.Any(x => x.Username == userParam.Username))
                    throw new ApplicationException("Username" + userParam.Username + "is already taken");

                user.Username = userParam.Username;

            }

            //update user properties if provided
            if (!string.IsNullOrEmpty(userParam.FirstName))
                user.FirstName = userParam.FirstName;

            if (!string.IsNullOrEmpty(userParam.LastName))
                user.LastName = userParam.LastName;

            if (string.IsNullOrEmpty(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _context.Users.Update(user);
            _context.SaveChanges();
        }

        //helper methods
        //2 objects are created, a password hash a salt(secret key).
        //using is helpful because these objetcs wont be reused! momory cleaning and more performant
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null)
                throw new ArgumentNullException("Password");
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Value cannot be empty nor contain whitespaces", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null)
                throw new ArgumentNullException("password");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Value cannot be empty nor contain whitespaces", "password");
            if (storedHash.Length != 64)
                throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128)
                throw new ArgumentException("Invalid length of password hash (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

