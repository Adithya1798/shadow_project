using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Login_Application.Models
{
    public class LoginClass
    {   
        [Required(ErrorMessage = "Please Enter Username")]
        [Display(Name = "Username")]
        public string username { get; set; }
        [Required(ErrorMessage = "Please Enter password")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string password { get; set; }
    }
}