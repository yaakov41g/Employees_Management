using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{   // this class defines expiranse time for a specific token.
    public class CustomEmailConfirmationTokenProviderOptions
     : DataProtectionTokenProviderOptions
    { }
}
