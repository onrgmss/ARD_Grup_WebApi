namespace ARD_Grup_WebApi.Data
{
    public class TokenApi
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expiration {get; set; } 

        public string RoleName { get; set; }
    }
}
