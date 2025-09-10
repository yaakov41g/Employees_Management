using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Security
{
    public class DataProtectionPurposeStrings
    { // this string is used as an app side part of the encription compound in addition to the rootkey of the IDataProtectionProvider. 
        public readonly string EmployeeIdRouteValue = "EmployeeIdRouteValue";
    }
}
