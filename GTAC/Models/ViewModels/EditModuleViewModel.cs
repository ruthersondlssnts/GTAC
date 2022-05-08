using GTAC.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GTAC.Models.ViewModels
{
    public class EditModuleViewModel
    {
        [Key]
        public Guid Id { get; set; }

        public string Path { get; set; }
        [Required]
        public string Title { get; set; }
        public string UploaderId { get; set; }

        [FileExtension]
        [DisplayName("Module File")]
        public IFormFile ModuleUpload { get; set; }
    }
}
