using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{ // the (base) class creates token for email/password/etc confirmation. we use derivied class in order to set time expiration for a specific kind of token (like email or password etc token) instead of the default of the all
    public class CustomEmailConfirmationTokenProvider<TUser>
     : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public CustomEmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
                                        IOptions<CustomEmailConfirmationTokenProviderOptions> options, // this class parameter is our drived class that defines expiranse time for a specific token.
                                        ILogger<CustomEmailConfirmationTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {
        }

    }
}
