namespace ARD_Grup_WebApi.Models
{
    public class EditProfileModel
    {
        public string NameSurname { get; set; }
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
