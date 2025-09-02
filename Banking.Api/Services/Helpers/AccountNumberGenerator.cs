namespace Banking.Api.Services.Helpers
{
    public class AccountNumberGenerator : IAccountNumberGenerator
    {
        public string Generate()
        {
            // simple account number generator: YYYYMMDD + 6 random digits
            return DateTime.UtcNow.ToString("yyyyMMdd") + new Random().Next(100000, 999999).ToString();
        }
    }
}
