using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeofenceServer.Data
{
    public class TargetUser
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
            Id = -1;
        }

        public override string ToString()
        {
            return Email + Program.USER_SEPARATOR +
                Name + Program.USER_SEPARATOR +
                PasswordHash + Program.USER_SEPARATOR +
                LocationHandler.truncateHistoryForTransmission(LocationHistory) + Program.USER_SEPARATOR
                + Id;
        }
    }

    public class TargetUserDbContext : DbContext
    {
        public DbSet<TargetUser> Users { get; set; }
    }
}
