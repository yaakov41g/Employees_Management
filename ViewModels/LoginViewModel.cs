using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
        // the next  two properties are for using an External Authantication Provider (like google , facebook ,etc)
        //-  see https://csharp-video-tutorials.blogspot.com/2019/09/aspnet-core-google-authentication.html (P. 106 of Pragim - ASP.net core with MVC)
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}
