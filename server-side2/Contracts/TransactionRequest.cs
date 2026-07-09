using System.ComponentModel.DataAnnotations;

namespace SQLink.Contracts
{
    public class TransactionRequest
    {
        [Required]
        public string Account { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "USD";

        [Required]
        public string Status { get; set; } = "Success";
    }
}
