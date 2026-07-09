using System.Collections.Concurrent;

namespace SQLink
{
    public sealed class ConnectedClientTracker
    {
        private readonly ConcurrentDictionary<string, byte> _connections = new(StringComparer.Ordinal);

        public int Count => _connections.Count;

        public void Add(string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                return;
            }

            _connections.TryAdd(connectionId, 0);
        }

        public void Remove(string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                return;
            }

            _connections.TryRemove(connectionId, out _);
        }
    }
}
