using Banking.Api.Models;

namespace Banking.Api.Services
{
    public interface IAccountService
    {
        Task<Account> CreateAccountAsync(string ownerName, decimal initialBalance);
        Task<Account?> GetByAccountNumberAsync(string accountNumber);
        Task<IEnumerable<Account>> GetAllAccountsAsync();
        Task DepositAsync(string accountNumber, decimal amount);
        Task WithdrawAsync(string accountNumber, decimal amount);
        Task TransferAsync(string senderAccountNumber, string receiverAccountNumber, decimal amount);

    }
}
