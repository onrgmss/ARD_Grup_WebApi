using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ARD_Grup_WebApi.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Name and surname is required.")]
        [StringLength(30, ErrorMessage = "Name and surname can be max 30 characters.")]
        public string NameSurname { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password can't be less than 8 characters.")]
        [MaxLength(18, ErrorMessage = "Password can't be more than 18 characters.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Re-Password is required.")]
        [MinLength(8, ErrorMessage = "Password can't be less than 8 characters.")]
        [MaxLength(18, ErrorMessage = "Password can't be more than 18 characters.")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string RePassword { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [MaxLength(14)]
        [MinLength(10)]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "At least one role is required.")]
        public List<string> Roles { get; set; }

        [Required(ErrorMessage = "Team Name is required.")]
        public string TeamName { get; set; }
    }
}
