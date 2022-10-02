﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeofenceServer.Data
{
    public class OverseerUser
    {
        [Key]
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email missing while manipulating database", ErrorMessageResourceName = "Email")]
        [MaxLength(50, ErrorMessage = "Email adress was over 50 characters.")]
        [Index(IsUnique = true)]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Name missing while manipulating database", ErrorMessageResourceName = "Name")]
        [MaxLength(50, ErrorMessage = "Name adress was over 50 characters.")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password hash missing while manipulating database", ErrorMessageResourceName = "PasswordHash")]
        public string PasswordHash { get; set; }
        public string TrackedUserIDs { get; set; }

        public OverseerUser(string Email, string Name, string PasswordHash)
        {
            this.Email = Email;
            this.Name = Name;
            this.PasswordHash = PasswordHash;
        }

        public OverseerUser()
        {
            Email = "";
            Name = "";
            PasswordHash = "";
        }

        public override string ToString()
        {
            return Email + Program.USER_SEPARATOR + Name + Program.USER_SEPARATOR + PasswordHash + Program.USER_SEPARATOR + Id;
        }
    }

    public class OverseerUserDbContext : DbContext
    {
        public DbSet<OverseerUser> Users { get; set; }
    }
}
