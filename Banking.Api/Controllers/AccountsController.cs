using Banking.Api.Models;
using Banking.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _service;
        public AccountsController(IAccountService service)
        {
            _service = service;
        }

        // POST: /api/accounts — create account (body: ownerName, initialBalance)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequest req)
        {
            var created = await _service.CreateAccountAsync(req.OwnerName, req.InitialBalance);
            return CreatedAtAction(nameof(GetByNumber), new {accountNumber = created.AccountNumber}, ToDto(created));
        }

        // GET: /api/accounts/{accountNumber} — get account
        [HttpGet("{accountNumber}")]
        public async Task<IActionResult> GetByNumber(string accountNumber)
        {
            var acc = await _service.GetByAccountNumberAsync(accountNumber);
            if (acc == null) return NotFound();
            return Ok(ToDto(acc));
        }

        // GET: /api/accounts — list all
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAccountsAsync();
            return Ok(list.Select(ToDto));
        }

        // POST: /api/accounts/{accountNumber}/deposit — deposit (body: amount)
        [HttpPost("{accountNumber}/deposit")]
        public async Task<IActionResult> Deposit(string accountNumber, [FromBody] TransactionRequest req)
        {
            try
            {
                await _service.DepositAsync(accountNumber, req.Amount);
                return Ok();
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (ArgumentException e) { return BadRequest(e.Message); }
        }

        // POST: /api/accounts/{accountNumber}/withdraw — withdraw (body: amount)
        [HttpPost("{accountNumber}/withdraw")]
        public async Task<IActionResult> Withdraw(string accountNumber, [FromBody] TransactionRequest req)
        {
            try
            {
                await _service.WithdrawAsync(accountNumber, req.Amount);
                return Ok();
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (ArgumentException e) { return BadRequest(e.Message); }
            catch (InvalidOperationException e) { return BadRequest(e.Message); }
        }

        // POST: /api/accounts/transfer — transfer (body: fromAccountNumber, toAccountNumber, amount)
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest req)
        {
            try
            {
                await _service.TransferAsync(req.FromAccountNumber, req.ToAccountNumber, req.Amount);
                return Ok();
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (ArgumentException e) { return BadRequest(e.Message); }
            catch (InvalidOperationException e) { return BadRequest(e.Message); }
        }
        // convert to data transfer object without internal DB stuff
        private static AccountDto ToDto(Account a) => new AccountDto
        {
            AccountNumber = a.AccountNumber,
            OwnerName = a.OwnerName,
            Balance = a.Balance,
            CreatedAt = a.CreatedAt
        };
    }

    public record CreateAccountRequest(string OwnerName, decimal InitialBalance);
    public record TransactionRequest(decimal Amount);
    public record TransferRequest(string FromAccountNumber, string ToAccountNumber, decimal Amount);
    public record AccountDto { 
        public string AccountNumber { get; init; } = null!; 
        public string OwnerName { get; init; } = null!; 
        public decimal Balance { get; init; } 
        public DateTime CreatedAt { get; init; } 
    }

}
