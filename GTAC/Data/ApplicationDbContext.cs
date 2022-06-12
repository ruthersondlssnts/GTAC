using GTAC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTAC.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<RequestReschedule> RequestReschedules { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Quiz_Student> Quiz_Students { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<StudentChangeRequest> StudentChangeRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.SetNull;
            }
        }
    }
}
