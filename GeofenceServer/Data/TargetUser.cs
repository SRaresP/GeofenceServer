using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace GeofenceServer.Data
{
    public class TargetUser
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
        public string LocationHistory { get; set; }
        public int NrOfCodeGenerations { get; set; }

        public TargetUser(string Email, string Name, string PasswordHash, int nrOfCodeGenerations = 0, string LocationHistory = "")
        {
            this.Email = Email;
            this.Name = Name;
            this.PasswordHash = PasswordHash;
            this.NrOfCodeGenerations = nrOfCodeGenerations;
            this.LocationHistory = LocationHistory;
        }

        public TargetUser()
        {
            Email = "";
            Name = "";
            PasswordHash = "";
            LocationHistory = "";
        }

        public override string ToString()
        {
            return Email + Program.COMM_SEPARATOR + Name + Program.COMM_SEPARATOR + PasswordHash + Program.COMM_SEPARATOR + LocationHistory;
        }
    }

    public class TargetUserDbContext : DbContext
    {
        public DbSet<TargetUser> Users { get; set; }
    }
}
