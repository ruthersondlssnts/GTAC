using GTAC.Helpers;
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
    public class Module
    {
        [Key]
        public Guid Id { get; set; }

        public string Path { get; set; }
        [Required]
        public string Title { get; set; }
        public string UploaderId { get; set; }
        [ForeignKey("UploaderId")]
        public User Uploader { get; set; }

        [FileExtension]
        [Required]
        [NotMapped]
        [DisplayName("Module File")]
        public IFormFile ModuleUpload { get; set; }
    }
}
