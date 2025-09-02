using Banking.Api.Data;
using Banking.Api.Models;
using Banking.Api.Services;
using Banking.Api.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace Banking.Api.Tests
{
    public class AccountServiceTests
    {
        private BankingDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<BankingDbContext>()
                .UseInMemoryDatabase(dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // ignore the TransactionIgnoredWarning
                .Options;
            return new BankingDbContext(options);
        }

        private IAccountNumberGenerator MockGenerator(string number = "ACC123")
        {
            var mock = new Mock<IAccountNumberGenerator>();
            mock.Setup(g => g.Generate()).Returns(number);
            return mock.Object;
        }

        // -------- CreateAccountAsync --------
        [Fact]
        public async Task CreateAccount_Succeeds_WhenValid()
        {
            using var db = CreateContext(nameof(CreateAccount_Succeeds_WhenValid));
            var service = new AccountService(db, MockGenerator("ACC001"));

            var acc = await service.CreateAccountAsync("Alice", 100m);

            Assert.Equal("ACC001", acc.AccountNumber);
            Assert.Equal(100m, acc.Balance);
            Assert.Single(db.Accounts);
        }

        [Fact]
        public async Task CreateAccount_Throws_WhenNegativeBalance()
        {
            using var db = CreateContext(nameof(CreateAccount_Throws_WhenNegativeBalance));
            var service = new AccountService(db, MockGenerator());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateAccountAsync("Bob", -10m));
        }

        // -------- GetAllAccountsAsync --------
        [Fact]
        public async Task GetAllAccounts_ReturnsAccounts()
        {
            using var db = CreateContext(nameof(GetAllAccounts_ReturnsAccounts));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "A1", OwnerName = "Test", Balance = 50, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            var accounts = await service.GetAllAccountsAsync();

            Assert.Single(accounts);
        }

        // -------- GetByAccountNumberAsync --------
        [Fact]
        public async Task GetByAccountNumber_ReturnsAccount_WhenExists()
        {
            using var db = CreateContext(nameof(GetByAccountNumber_ReturnsAccount_WhenExists));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "A2", OwnerName = "Test", Balance = 50, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            var acc = await service.GetByAccountNumberAsync("A2");

            Assert.NotNull(acc);
            Assert.Equal("A2", acc!.AccountNumber);
        }

        [Fact]
        public async Task GetByAccountNumber_ReturnsNull_WhenNotExists()
        {
            using var db = CreateContext(nameof(GetByAccountNumber_ReturnsNull_WhenNotExists));
            var service = new AccountService(db, MockGenerator());

            var acc = await service.GetByAccountNumberAsync("UNKNOWN");

            Assert.Null(acc);
        }

        // -------- DepositAsync --------
        [Fact]
        public async Task Deposit_IncreasesBalance_WhenValid()
        {
            using var db = CreateContext(nameof(Deposit_IncreasesBalance_WhenValid));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "A3", OwnerName = "Test", Balance = 50, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            await service.DepositAsync("A3", 25m);

            var acc = db.Accounts.Single(a => a.AccountNumber == "A3");
            Assert.Equal(75m, acc.Balance);
        }

        [Fact]
        public async Task Deposit_Throws_WhenNegativeAmount()
        {
            using var db = CreateContext(nameof(Deposit_Throws_WhenNegativeAmount));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "A4", OwnerName = "Test", Balance = 50, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.DepositAsync("A4", -10m));
        }

        [Fact]
        public async Task Deposit_Throws_WhenAccountNotFound()
        {
            using var db = CreateContext(nameof(Deposit_Throws_WhenAccountNotFound));
            var service = new AccountService(db, MockGenerator());

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.DepositAsync("UNKNOWN", 50m));
        }

        // -------- WithdrawAsync --------
        [Fact]
        public async Task Withdraw_DecreasesBalance_WhenValid()
        {
            using var db = CreateContext(nameof(Withdraw_DecreasesBalance_WhenValid));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "A5", OwnerName = "Test", Balance = 100, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            await service.WithdrawAsync("A5", 40m);

            var acc = db.Accounts.Single(a => a.AccountNumber == "A5");
            Assert.Equal(60m, acc.Balance);
        }

        [Fact]
        public async Task Withdraw_Throws_WhenAmountNonPositive()
        {
            using var db = CreateContext(nameof(Withdraw_Throws_WhenAmountNonPositive));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "A6", OwnerName = "Test", Balance = 100, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.WithdrawAsync("A6", 0m));
        }

        [Fact]
        public async Task Withdraw_Throws_WhenAccountNotFound()
        {
            using var db = CreateContext(nameof(Withdraw_Throws_WhenAccountNotFound));
            var service = new AccountService(db, MockGenerator());

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.WithdrawAsync("UNKNOWN", 20m));
        }

        [Fact]
        public async Task Withdraw_Throws_WhenInsufficientFunds()
        {
            using var db = CreateContext(nameof(Withdraw_Throws_WhenInsufficientFunds));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "A7", OwnerName = "Test", Balance = 10, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.WithdrawAsync("A7", 50m));
        }

        // -------- TransferAsync --------
        [Fact]
        public async Task Transfer_MovesFunds_WhenValid()
        {
            using var db = CreateContext(nameof(Transfer_MovesFunds_WhenValid));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "SRC", OwnerName = "Sender", Balance = 100, CreatedAt = DateTime.UtcNow });
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "DST", OwnerName = "Receiver", Balance = 20, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            await service.TransferAsync("SRC", "DST", 50m);

            Assert.Equal(50m, db.Accounts.Single(a => a.AccountNumber == "SRC").Balance);
            Assert.Equal(70m, db.Accounts.Single(a => a.AccountNumber == "DST").Balance);
        }

        [Fact]
        public async Task Transfer_Throws_WhenAmountNonPositive()
        {
            using var db = CreateContext(nameof(Transfer_Throws_WhenAmountNonPositive));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "SRC2", OwnerName = "Sender", Balance = 100, CreatedAt = DateTime.UtcNow });
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "DST2", OwnerName = "Receiver", Balance = 100, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.TransferAsync("SRC2", "DST2", 0m));
        }

        [Fact]
        public async Task Transfer_Throws_WhenSameAccount()
        {
            using var db = CreateContext(nameof(Transfer_Throws_WhenSameAccount));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "ACCX", OwnerName = "User", Balance = 100, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.TransferAsync("ACCX", "ACCX", 10m));
        }

        [Fact]
        public async Task Transfer_Throws_WhenSenderNotFound()
        {
            using var db = CreateContext(nameof(Transfer_Throws_WhenSenderNotFound));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "DSTY", OwnerName = "Receiver", Balance = 100, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.TransferAsync("MISSING", "DSTY", 10m));
        }

        [Fact]
        public async Task Transfer_Throws_WhenReceiverNotFound()
        {
            using var db = CreateContext(nameof(Transfer_Throws_WhenReceiverNotFound));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "SRCY", OwnerName = "Sender", Balance = 100, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.TransferAsync("SRCY", "MISSING", 10m));
        }

        [Fact]
        public async Task Transfer_Throws_WhenInsufficientFunds()
        {
            using var db = CreateContext(nameof(Transfer_Throws_WhenInsufficientFunds));
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "SRCZ", OwnerName = "Sender", Balance = 10, CreatedAt = DateTime.UtcNow });
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "DSTZ", OwnerName = "Receiver", Balance = 10, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new AccountService(db, MockGenerator());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.TransferAsync("SRCZ", "DSTZ", 50m));
        }
    }
}