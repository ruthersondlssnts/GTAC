using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GTAC.Models
{
    public class Quiz_Student
    {
        [Key]
        public Guid Id { get; set; }

        public Guid QuizId { get; set; }
        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }

        public Guid StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }
}
