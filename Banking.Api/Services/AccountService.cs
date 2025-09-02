using Banking.Api.Data;
using Banking.Api.Models;
using Banking.Api.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Banking.Api.Services
{
    public class AccountService : IAccountService
    {
        private readonly BankingDbContext _db; 
        private readonly IAccountNumberGenerator _generator;
        public AccountService(BankingDbContext db, IAccountNumberGenerator generator) 
        {
            _db = db;
            _generator = generator;
        }

        public async Task<Account> CreateAccountAsync(string ownerName, decimal initialBalance)
        {
            if (initialBalance < 0) throw new ArgumentException("Initial balance cannot be negative!");
            var account = new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = _generator.Generate(),
                OwnerName = ownerName,
                Balance = initialBalance,
                CreatedAt = DateTime.UtcNow,
            };
            _db.Accounts.Add(account);
            await _db.SaveChangesAsync();
            return account;
        }

        public Task<IEnumerable<Account>> GetAllAccountsAsync()
            => Task.FromResult<IEnumerable<Account>>(_db.Accounts.AsNoTracking().ToList()); //no tracking to prevent modifiyng accounts

        public Task<Account?> GetByAccountNumberAsync(string accountNumber)
            => _db.Accounts.SingleOrDefaultAsync(a => a.AccountNumber == accountNumber);

        public async Task DepositAsync(string accountNumber, decimal amount)
        {
            if (amount < 0) throw new ArgumentException("Amount cannot be negative number or zero!");
            var acc = await _db.Accounts.SingleOrDefaultAsync(a => a.AccountNumber == accountNumber)
                ?? throw new KeyNotFoundException("Account not found!");
            acc.Balance += amount;
            await _db.SaveChangesAsync();
        }

        public async Task TransferAsync(string senderAccountNumber, string receiverAccountNumber, decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount cannot be negative number or zero!");
            if (senderAccountNumber == receiverAccountNumber) throw new ArgumentException("Source and destination must differ");

            // We're gonna use DB transaction to make this atomic
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var src = await _db.Accounts.SingleOrDefaultAsync(a => a.AccountNumber == senderAccountNumber)
                    ?? throw new KeyNotFoundException("Sender account not found!");
                
                var dst = await _db.Accounts.SingleOrDefaultAsync(a => a.AccountNumber == receiverAccountNumber)
                    ?? throw new KeyNotFoundException("Receiver account not found!");

                if (src.Balance < amount) throw new InvalidOperationException("Insufficient funds in source account!");

                src.Balance -= amount;
                dst.Balance += amount;

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //concurrency handling
                throw new InvalidOperationException("Concurrency conflict while transfering funds. Try again later.");
            }
        }

        public async Task WithdrawAsync(string accountNumber, decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount cannot be negative number or zero!");
            var acc = await _db.Accounts.SingleOrDefaultAsync(a => a.AccountNumber == accountNumber)
                ?? throw new KeyNotFoundException("Account not found");
            if (acc.Balance < amount) throw new InvalidOperationException("Insufficient funds!");
            acc.Balance -= amount;
            await _db.SaveChangesAsync();
        }

    }
}
