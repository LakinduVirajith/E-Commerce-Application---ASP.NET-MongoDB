namespace E_Commerce_Application___ASP.NET_MongoDB.Interfaces
{
    public interface ICommonService
    {
        string HashPassword(string password);
        string GenerateActivationToken();
        bool IsVerifyPassword(string storedPassword, string inputPassword);
        bool IsValidMacAddress(string macAddress);
        string? ValidateDto<T>(T dto);
        void MapProperties<TSource, TDestination>(TSource source, TDestination destination);
    }
}
