using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GTAC.Models
{
    public class Quiz
    {
        [Key]
        public Guid Id { get; set; }
        public string Link { get; set; }
        public Guid AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; }
    }
}
