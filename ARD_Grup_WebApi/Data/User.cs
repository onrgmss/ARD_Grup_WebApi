using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARD_Grup_WebApi.Data
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string NameSurname { get; set; } 
        [EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public bool Locked { get; set; }
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
        public virtual Team Teams { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public bool IsLeader {get; set; }
        public string ProfilePhotoPath { get; set; } 
        [NotMapped]
        public IFormFile ProfilePhoto { get; set; }

    }
}
