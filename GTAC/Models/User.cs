﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GTAC.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        [Required]
        public string Middlename { get; set; }
        public string Suffix { get; set; }
        public bool? IsActivated { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public DateTime Birthday { get; set; }
        public string PhotoPath { get; set; }
        public bool? IsProfileApproved { get; set; }

    }
}
