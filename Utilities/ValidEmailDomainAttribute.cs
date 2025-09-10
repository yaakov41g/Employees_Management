using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Utilities
{ // this class binded by its name to the Custom Validation Attribute of the 'Email' in the RegisterViewModel 
    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        private readonly string allowedDomain;

        public ValidEmailDomainAttribute(string allowedDomain) // there is need to be a matching between the parameter name here and this which in the attribute of 'Email'
        {
            this.allowedDomain = allowedDomain;
        }

        public override bool IsValid(object value) // this is called during the validating proccess
        { // any legal email pattern is splitted to two parts : one to the right of '@' and the other to the left
            string[] strings = value.ToString().Split('@');//- then the two strings assigned into 'strings' array         
            return strings[1].ToUpper() == allowedDomain.ToUpper();
        }
    }
}
