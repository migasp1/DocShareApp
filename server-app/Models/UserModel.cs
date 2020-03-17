using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocShareApp.Models
{
    //This is a response model that defines the data that is return for the GET
    //requests to the /users(get all of them) and /users/{id} routes of the api.
    //The methods GetAll and GetById methods of the UsersController convert entity
    //data into user model data before returning in the response, so that some 
    //properties - password hash and password - dont get sent back.
    public class UserModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
    }
}
