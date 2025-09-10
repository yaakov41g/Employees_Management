using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
namespace EmployeeManagement.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [Required, MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        // attribute that signs the property (EncryptedId) as non binded to the the dbase connected table , to avoid error
        [NotMapped]
        public string EncryptedId { get; set; } // contains the encripted user id. Used for displaying the 'id' part of the route, encripted.
        public string Name { get; set; }
        [Display(Name = "Office Email")] // replace the lable of Name
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",
            ErrorMessage = "Invalid email format")]
        [Required]
        public string Email { get; set; }
        [Required]
        public Dept? Department { get; set; } // the ? sign makes this enum property as nullable
        // so it will not invoke the original error massage of 'value "" ' (the type of enum  fields are intetger)
        //public string PhotoPath { get; internal set; }
        public string PhotoPath { get; set; } = "";

    }
}
