using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Banking.Api.Models
{
    public class Account
    {
        [Key]
        public Guid Id {  get; set; }
        [Required]
        public string AccountNumber { get; set; } = null!; //unique human-readable / IBAN-like string
        public string OwnerName { get; set; } = null!;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        [Timestamp]
        public byte[]? RowVersion {  get; set; } // optimistic concurrency token
    }
}
