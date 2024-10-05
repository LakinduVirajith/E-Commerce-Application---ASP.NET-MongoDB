namespace E_Commerce_Application___ASP.NET_MongoDB.Interfaces
{
    public interface IMailService
    {
        Task<bool> SendActivationEmailAsync(string toMail, string userName, string token);
    }
}
