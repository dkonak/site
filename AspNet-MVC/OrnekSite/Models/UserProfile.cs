﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OrnekSite.Models
{
    public class UserProfile
    {
        public string Id { get; set; }
        [Required] 
        [DisplayName("Adı")] 
        public string Name { get; set; }
        [Required]
        [DisplayName("Soyadı")]
        public string Surname { get; set; }
        [Required]
        [DisplayName("Kullanıcı Adı")]
        public string Username { get; set; }
        [Required]
        [DisplayName("Email")]
        [EmailAddress(ErrorMessage = "Geçersiz Email Adresi...")]
        public string Email { get; set; }
    }

}