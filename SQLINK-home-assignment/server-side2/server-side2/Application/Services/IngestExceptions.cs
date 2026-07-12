namespace SQLink.Services
{
    public sealed class TransactionConflictException : Exception
    {
        public TransactionConflictException(string id, Exception? innerException = null)
            : base($"Transaction with id '{id}' already exists.", innerException)
        {
            Id = id;
        }

        public string Id { get; }
    }

    public sealed class RealtimePublishException : Exception
    {
        public RealtimePublishException(string id, Exception? innerException = null)
            : base($"Transaction '{id}' persisted but real-time publish failed.", innerException)
        {
            Id = id;
        }

        public string Id { get; }
    }
}