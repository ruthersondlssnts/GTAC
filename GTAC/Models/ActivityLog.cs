using GTAC.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GTAC.Models
{
    public class ActivityLog
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public Area Area { get; set; }
        [Required]
        public Action Action { get; set; }

        [Required]
        public string Description { get; set; }

        public static void Create(string userId, Area area, Action action, string desc, ApplicationDbContext context)
        {
            var activityLog = new ActivityLog
            {
                Id = new Guid(),
                Area = area,
                Action = action,
                UserId = userId,
                CreatedAt = DateTime.Now,
                Description = desc
            };
            context.ActivityLogs.Add(activityLog);
            context.SaveChanges();
        }
    }
}
