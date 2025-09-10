using EmployeeManagement.Models.EmployeeManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EmployeeManagement.Models
{                      //
    //public class AppDbContext : IdentityDbContext // inherits (by end of inheritence chain) from DbContext, and provides mannnagement for db identity details
   // {  // this is like a register for the database connection , i think
    public class AppDbContext : IdentityDbContext<ApplicationUser> //ApplicationUser is here to say that a new migration need deal 
    {   //- with the extended class ApplicationUser instead of with its base (IdentityUser which has not the 'City' property)
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        // DbSet is , apparently parallel to List of in-memmory(mock, not stored in Dbase)
        public DbSet<Employee> Employees { get; set; }
        //in this method , I think , the new migration is tied to the Epmloyee data structure (look at the seed()). by the way , in case of Identity data table/structure , this 
        protected override void OnModelCreating(ModelBuilder modelBuilder)        //- is done (I mean the tying of the migratin) directly to the Identity User structure.
        {// In order to add the identityMigration which need deal with identity keys , we need call the base OnModelCreating method
            base.OnModelCreating(modelBuilder);            // the meaning of 'builder' here is building table/model in Dbase, this is not the compilation builder.
            // here we determine the "Enforce ON DELETE NO ACTION" policy which means that we can't delete a member of table (like role or user for example) if it has connection by key field to another table. for example
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))  // we can't delete role if it still has users ,we need to delete his users registering before. Now , when we make new migration here, we can see 
            {                                                          // the new policy on the property view(window) of the foregn keys. the new policy-  on delete : 'no action' instead of 'cascade'. you can get and see by clicking Menu -View->Sql server object explorer : in the DB tables of employee rihgt click on->  
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;        //- 'AspNetUserRoles'->View Designer @@ See on the course : https://www.pragimtech.com/courses/asp-net-core-mvc-tutorial-for-beginners/   item 89.
            }                                                                    //- The migration for "Enforce ON DELETE NO ACTION" policy has the name "DB_KeyDepend"
            /*   modelBuilder.Entity<Employee>().HasData(
             new Employee
             {
                 Id = 1,
                 Name = "Mark",
                 Department = Dept.IT,
                 Email = "mark@pragimtech.com"
             }
             // see this data records in ModelBuilderExtensions class
            new Employee
              {
                  Id = 1,
                  Name = "Mary",
                  Department = Dept.IT,
                  Email = "mary@pragimtech.com"
              },
              new Employee
              {
                  Id = 2,
                  Name = "John",
                  Department = Dept.HR,
                  Email = "john@pragimtech.com"
              }

             );*/
            modelBuilder.Seed();
        }

    }
}
