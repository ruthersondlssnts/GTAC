using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GTAC.Models
{
    public class StudentChangeRequest
    {
        [Key]
        public Guid Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Middlename { get; set; }
        public string Suffix { get; set; }
        public string Address { get; set; }
        public DateTime Birthday { get; set; }
        public string PhotoPath { get; set; }
        public string PhoneNumber { get; set; }
        public Status Status { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
