using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GTAC.Models
{
    public class Certificate
    {
        [Key]
        public Guid Id { get; set; }
        public string Path { get; set; }
        public string AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public User Author { get; set; }
        public string ApproverId { get; set; }
        [ForeignKey("ApproverId")]
        public User Approver { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public Status Status { get; set; }

        [Required]
        [NotMapped]
        [DisplayName("Certificate File")]
        public IFormFile CertificateUpload { get; set; }
    }
}
