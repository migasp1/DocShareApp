using DocShareApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocShareApp.Helpers;
using DocShareApp.Models;
using DocShareApp.Mapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        public User Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            //Now we access the users through DbSet class implemented in DataContext
            //Culture is a language-english for example. 
            //We can apply LINQ queries to every IEnumerable! IQueryble implements IEnumerable!
            //var user = _context.Users.Where(x => x.Username.StartsWith("m", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Username);
            //var user1 = _context.Users.Where(x => x.Username.StartsWith("m")).Select(x => x.Username);
            var user = _context.Users.SingleOrDefault(x => x.Email == email);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            //pw hash kept in db
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }

        public User Create(RegisterModel registerModel)
        {
            if (string.IsNullOrEmpty(registerModel.Password))
                throw new ArgumentException("Password is required");
            if (_context.Users.Any(x => x.Email == registerModel.Email))
                throw new ApplicationException("Email \"" + registerModel.Email + "\" already exists");

            CreatePasswordHash(registerModel.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = _mapper.MapRegisterModel(registerModel, passwordHash, passwordSalt);
            user.Role = Role.User;
            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public void ChangePassword(ChangePasswordModel changePasswordModel, int userId)
        {
            if (changePasswordModel.OldPassword.Equals(changePasswordModel.NewPassword))
                throw new ArgumentException("New password must differ from the old password");

            var user = _context.Users.SingleOrDefault(user => user.Id.Equals(userId));

            if (user == null)
                throw new ApplicationException("User not found");

            var oldPasswordHashMatches = VerifyPasswordHash(changePasswordModel.OldPassword, user.PasswordHash, user.PasswordSalt);

            if (!oldPasswordHashMatches)
                throw new ApplicationException("Old Password is not correct");

            CreatePasswordHash(changePasswordModel.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void ChangeNamesUser(ChangeNameUserModel changeNameUserModel, int userId)
        {
            if (string.IsNullOrEmpty(changeNameUserModel.NewFirstName) && string.IsNullOrEmpty(changeNameUserModel.NewLastName))
                throw new ApplicationException("At least one of the fields must be fullfield");

            var user = _context.Users.SingleOrDefault(user => user.Id.Equals(userId));

            if (user == null)
                throw new ApplicationException("User not found");

            if (user.FirstName.Equals(changeNameUserModel.NewFirstName) && user.LastName.Equals(changeNameUserModel.NewLastName))
                throw new ApplicationException("First and last name are the same as before");

            if (!string.IsNullOrEmpty(changeNameUserModel.NewFirstName))
                user.FirstName = changeNameUserModel.NewFirstName;

            if (!string.IsNullOrEmpty(changeNameUserModel.NewLastName))
                user.LastName = changeNameUserModel.NewLastName;

            _context.Users.Update(user);
            _context.SaveChanges();
        }

        //lista de nomes
        //primeiro e ultimo nome
        //ordem alfabetica ascendente p/ primeiro, descendente p/seg
        public IEnumerable<string> GetAllUsernameSpecial()
        {
            var list = _context.Users.OrderBy(user => user.FirstName).ThenByDescending(user => user.LastName).Select(user => user.FirstName + " " + user.LastName);
            return list;
        }

        //usernames
        //firstame começa por m
        //ordenados por id (int)
        public IEnumerable<string> GetAllUsernameSpecial2()
        {
            var list = _context.Users.Where(user => user.Email.StartsWith("m")).OrderBy(user => user.Id).Select(user => user.FirstName);
            return list;
        }

        //todos os apelidos
        //associados a um primeiro nome
        //list por ordem alfabetica
        //Dictionary (key=primeiro nome, value = list<string>(apelidos)
        //Groupby e ToDictionary
        //nao ha ha elementos duplicados(ja na resposta)

        public Dictionary<string, List<string>> GetAllSurnamesVerySpezialeByMihail()
        {

            Dictionary<string, List<string>> dictionary = _context.Users.GroupBy(user => user.FirstName, StringComparer.CurrentCultureIgnoreCase).ToDictionary(group => group.Key, group => group.OrderBy(user => user.LastName)
            .Select(user => user.LastName).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList());

            return dictionary;
        }

        //primeiros 10 first e last names
        //ordenados por ordem alfabetica
        //first e last acabam em "a"

        public IEnumerable<string> GetFirstTenSpecial()
        {
            var list = _context.Users.Where(user => user.FirstName.EndsWith("a", StringComparison.CurrentCultureIgnoreCase) && user.LastName.EndsWith("a", StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(user => user.FirstName).ThenBy(user => user.LastName).Select(user => user.FirstName + " " + user.LastName).Take(10);
            return list;
        }

        //todos os users
        //nomes repetidos
        //dicionario chave first e last name
        //value = numero total de pessoas com o nome da chave 

        public Dictionary<string, int> GetSumOfAllFirstAndLastNames()
        {
            var dictionary = _context.Users.GroupBy(user => user.FirstName + " " + user.LastName, StringComparer.CurrentCultureIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Key.Count());
            return dictionary;
        }

        //dicionario 
        // chave = carater
        //value = First Names associados ao carater da key
        //se houver so um, CAGUEI

        public Dictionary<char, List<string>> GetAllFirstNamesSpecial()
        {
            Dictionary<char, List<string>> dictionary = _context.Users.GroupBy(user => user.FirstName.First()).Where(group => group.Count() > 1)
                .ToDictionary(group => group.Key, group => group.Select(user => user.FirstName).ToList());
            return dictionary;
        }

        //query em que dê um dicionario em que tens como chave um inteiro, que é o numero de 
        //letras do primeiro nome, e como valor uma lista de todos os primeiros 
        //nomes com esse numero de letras
        // por ordem alfabetica os nomes
        //e case insensitive
        public Dictionary<int, List<string>> GetAllNumberOfOcurrencesSpecial()
        {
            Dictionary<int, List<string>> dictionary = _context.Users.GroupBy(user => user.FirstName.Count())
                .ToDictionary(group => group.Key, group => group.OrderBy(user => user.FirstName)
                .Select(user => user.FirstName).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList());
            return dictionary;
        }

        //total de nomes diferentes que 
        //ha na base de dados (comb diffs)
        public int GetAllNamesFromDB()
        {
            return _context.Users.Select(user => user.FirstName + " " + user.LastName).Distinct(StringComparer.InvariantCultureIgnoreCase).Count();
        }

        //todos os nomes
        //letra do meio do primeiro nome e qqlr coisa
        //e para cada letra (do m prim nome)
        //o tamanho de carateres do primeiro nome 
        //nao ha duplicados na string final concatenada
        //InvariantCultureIgnoreCase
        //nova func: SelectMany e ElementAt
        public Dictionary<char, int> GetSuperSpecialByMihail()
        {
            var dictionary = _context.Users.GroupBy(user => user.FirstName.ElementAt(user.FirstName.Length / 2))
                .ToDictionary(group => group.Key, group => group.Select(user => user.FirstName).Distinct(StringComparer.InvariantCultureIgnoreCase).
                SelectMany(user => user).Count());
            return dictionary;
        }

        //helper methods
        //2 objects are created, a password hash a salt(secret key).
        //using is helpful because these objetcs wont be reused! memory cleaning and more performant
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

        public User RetrievePersonalUserInfo(int id)
        {
            User user = _context.Users.SingleOrDefault(user => user.Id.Equals(id));
            if (user == null)
                throw new ApplicationException("User does not exist");
            return user;
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users.Select(user => user);
        }

        public void Delete(int id)
        {
            User user = _context.Users.SingleOrDefault(user => user.Id.Equals(id));
            if (user == null)
                throw new ApplicationException("User does not exist");

            EntityEntry<User> removedUser = _context.Users.Remove(user);

            _context.SaveChanges();
        }
    }
}

