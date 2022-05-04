using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GTAC.Models
{
    public class Schedule
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Time { get; set; }

        public Guid StudentId { get; set; }
        public string Concerns { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Status Status { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }
}
