namespace SQLink.Contracts
{
    public class TransactionResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
