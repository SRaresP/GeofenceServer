using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace GeofenceServer.Data
{
    public class OverseerUser
    {
        [Key]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email missing while manipulating database", ErrorMessageResourceName = "Email")]
        [MaxLength(50, ErrorMessage = "Email adress was over 50 characters.")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Name missing while manipulating database", ErrorMessageResourceName = "Name")]
        [MaxLength(50, ErrorMessage = "Name adress was over 50 characters.")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password hash missing while manipulating database", ErrorMessageResourceName = "PasswordHash")]
        public string PasswordHash { get; set; }

        public OverseerUser(string Email, string Name, string PasswordHash, string LocationHistory = "")
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
            return Email + Program.COMM_SEPARATOR + Name + Program.COMM_SEPARATOR + PasswordHash;
        }
    }

    public class OverseerUserDbContext : DbContext
    {
        public DbSet<OverseerUser> Users { get; set; }
    }
}
