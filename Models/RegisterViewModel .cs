using EmployeeManagement.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{[AllowAnonymous]
    public class RegisterViewModel
    {
        
        [Required]
        [EmailAddress]
        [Remote(action: "IsEmailInUse", controller: "Account")] //Remote() is a jquery method that makes ajax request from the register form on the client to the server
        [ValidEmailDomainAttribute(allowedDomain: "pragimtech.com", // this is a custom  Validation Attribute , its class is in Utilities/ValidEmailDomainAttribute.cs
        ErrorMessage = "Email domain must be pragimtech.com")]
        public string Email { get; set; }     //- in order to check if the new email is not already exist. see the "Account" controller/IsEmailInUse              

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password",
            ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public string City { get; set; }
    }
}
