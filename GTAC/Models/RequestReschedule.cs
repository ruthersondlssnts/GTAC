using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GTAC.Models
{
    public class RequestReschedule
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public DateTime DayOne { get; set; }
        [Required]
        public DateTime DayTwo { get; set; }

        [Required]
        public DateTime DayThree { get; set; }

        [Required]
        public DateTime DayFour { get; set; }

        [Required]
        public Guid ScheduleId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        [Required]
        public Status Status { get; set; }

        [ForeignKey("ScheduleId")]
        public virtual Schedule Schedule { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
