using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DocShareApp.Models
{
    public class RegisterModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name field must not be empty")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name field must not be empty")]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Email field must not be empty")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password field must not be empty")]
        public string Password { get; set; }
    }
}
