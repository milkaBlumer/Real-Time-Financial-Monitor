using System;
using System.ComponentModel.DataAnnotations;

namespace SQLink.Models
{
    public class Transaction
    {
        [Required]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Account { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "USD";

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public string Status { get; set; } = "Success"; // or Failed
    }
}
