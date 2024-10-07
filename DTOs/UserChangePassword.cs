namespace E_Commerce_Application___ASP.NET_MongoDB.DTOs
{
    public class UserChangePassword
    {
        public string CurrentPassword { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }
}
