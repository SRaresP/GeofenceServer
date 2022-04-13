using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace ServerExemplu.Data
{
    public class User
    {
        public static char Separator = '~';
        [Key]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email missing while manipulating database", ErrorMessageResourceName = "Email")]
        [MaxLength(50, ErrorMessage = "Email adress was over 50 characters.")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Name missing while manipulating database", ErrorMessageResourceName = "Name")]
        [MaxLength(50, ErrorMessage = "Name adress was over 50 characters.")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password hash missing while manipulating database", ErrorMessageResourceName = "PasswordHash")]
        public string PasswordHash { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Payment status missing while manipulating database", ErrorMessageResourceName = "PaymentDone")]
        public bool PaymentDone { get; set; }

        public User(string Email, string Name, string PasswordHash, bool PaymentDone)
        {
            this.Email = Email;
            this.Name = Name;
            this.PasswordHash = PasswordHash;
            this.PaymentDone = PaymentDone;
        }

        public User()
        {
            Email = "";
            Name = "";
            PasswordHash = "";
            PaymentDone = false;
        }

        public override string ToString()
        {
            return Email + Separator + Name + Separator + PasswordHash + Separator + PaymentDone.ToString();
        }
    }

    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    }
}
