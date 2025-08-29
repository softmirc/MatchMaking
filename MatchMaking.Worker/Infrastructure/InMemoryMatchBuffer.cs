namespace MatchMaking.Worker.Infrastructure
{
    public interface IMatchBuffer
    {
        void Add(string userId);
        int Count { get; }
        List<string> Take(int count);
        List<string> DequeueBatch(int count);
    }

    public class InMemoryMatchBuffer : IMatchBuffer
    {
        private readonly object _lock = new();
        private readonly Queue<string> _queue = new();

        public void Add(string userId)
        {
            lock (_lock)
            {
                if (!_queue.Contains(userId))
                {
                    _queue.Enqueue(userId);
                }
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        public List<string> Take(int count)
        {
            lock (_lock)
            {
                return _queue.Take(count).ToList();
            }
        }

        public List<string> DequeueBatch(int count)
        {
            lock (_lock)
            {
                var result = new List<string>();

                while (result.Count < count && _queue.Count > 0)
                {
                    result.Add(_queue.Dequeue());
                }

                return result;
            }
        }
    }
}
