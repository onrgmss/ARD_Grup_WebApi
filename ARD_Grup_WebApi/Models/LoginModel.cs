using System.ComponentModel.DataAnnotations;

namespace ARD_Grup_WebApi.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password can't be less than 8 characters.")]
        [MaxLength(18, ErrorMessage = "Password can't be more than 18 characters.")]
        public string Password { get; set; }
    }
}
