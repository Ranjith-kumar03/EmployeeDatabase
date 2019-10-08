using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Models
{
    //AppDbContext  to inherit IdentityDbContext (stop inheriting from DbContext)
    public class AppDbContext:IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {

        }
        public DbSet<Employee> Employees { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //we get error The entity type 'IdentityUserLogin<string>' requires a primary key to be defined. for solving this
            //Keys of Identity Table are mapped OnModelCreating method
            base.OnModelCreating(modelBuilder);
            modelBuilder.Seed();
            foreach(var foreignkey in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e=>e.GetForeignKeys()))
            {
                foreignkey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

    }
}
