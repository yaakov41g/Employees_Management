using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{ // here there are no validation attributes so in case of create empty form fields, an expection error is invoked.
    public class EmployeeCreateViewModel
    {
        //  [Required, MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public Dept? Department { get; set; }
        //# public IFormFile Photo { get; set; }
        public IFormFile Photo { get; set; }
    }
}
