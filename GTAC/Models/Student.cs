using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GTAC.Models
{
    public class Student
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime? EnrolledAt { get; set; }
        public DateTime? GraduatedAt { get; set; }
        [Required]
        public string UserId { get; set; }
        public string InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public virtual User Instructor { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
