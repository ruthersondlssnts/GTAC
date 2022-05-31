using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GTAC.Models
{
    public class Schedule
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [DisplayName("Day 1")]
        public DateTime DayOne { get; set; }
        [Required]
        [DisplayName("Day 2")]
        public DateTime DayTwo { get; set; }

        [Required]
        [DisplayName("Day 3")]
        public DateTime DayThree { get; set; }

        [Required]
        [DisplayName("Day 4")]
        public DateTime DayFour { get; set; }

        [Required]
        public Guid StudentId { get; set; }
        public bool? isDayOnePassed { get; set; }
        public bool? isDayTwoPassed { get; set; }
        public bool? isDayThreePassed { get; set; }
        public bool? isDayFourPassed { get; set; }

        public DateTime? ApprovedAt { get; set; }
        [Required]
        public Status Status { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }
}
