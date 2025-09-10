using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public class ApplicationUser : IdentityUser  // IdentityUser is a basic validation properties table/class /model which is dealt by
    {//- by the dbase/migratiom/app (by 'dbase/migratiom/app' I mean , you choose to call it out from the three, as you want)
        public string City { get; set; }
    }
}
