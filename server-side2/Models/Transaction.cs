using System;
using System.ComponentModel.DataAnnotations;

namespace SQLink.Models
{
    public class Transaction
    {
        [Required]
        [MinLength(1)]
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        [RegularExpression("^[A-Z]{3}$")]
        public string Currency { get; set; } = "USD";

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [RegularExpression("^(Pending|Completed|Failed)$")]
        public string Status { get; set; } = "Pending";
    }
}
