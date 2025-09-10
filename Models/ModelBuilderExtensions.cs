using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Models
{
    namespace EmployeeManagement.Models
    {// this is an extension of ModelBuilder from AppDbContext Model(class) , to fill here the data to seed,
     // then we call seed() from AppDbContext class to preseeding the data , this way keeps the code more organized and clean.
        public static class ModelBuilderExtensions
        {
            public static void Seed(this ModelBuilder modelBuilder) => modelBuilder.Entity<Employee>().HasData(
                        new Employee
                        {
                            Id = 1,
                            Name = "Mary",
                            Department = Dept.IT,
                            Email = "mary@pragimtech.com",
                            PhotoPath=null // "" is not good
                        },
                        new Employee
                        {
                            Id = 2,
                            Name = "John",
                            Department = Dept.HR,
                            Email = "john@pragimtech.com",
                            PhotoPath =null 
                        }
                    );
        }
    }
}
