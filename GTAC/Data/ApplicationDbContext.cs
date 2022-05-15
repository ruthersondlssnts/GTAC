using GTAC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
    }
}
