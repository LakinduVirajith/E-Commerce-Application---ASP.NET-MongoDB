namespace E_Commerce_Application___ASP.NET_MongoDB.Interfaces
{
    public interface IMailService
    {
        Task SendActivationEmailAsync(string toMail, string token);
    }
}
