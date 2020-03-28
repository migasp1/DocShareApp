using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DocShareApp.Models
{
    //Defining the model with the appropriate parameters for the incoming POST
    //request to the route /users/authenticate of the api. The data from the body
    //of the request is bounded to an AuthenticateModel instance and passed to the
    //method.
    public class AuthenticateModel
    {
        //This annotations let the api return an error message in case one or both
        //parameters and not filled.
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
