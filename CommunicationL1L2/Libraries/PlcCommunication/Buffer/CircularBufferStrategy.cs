using V3.S7Plc.Communication.Interfaces;

namespace V3.S7Plc.Communication.Buffer
{
    /// <summary>
    /// Circular buffer: keeps last N entries.
    /// </summary>
    public class CircularBufferStrategy : IBufferStrategy
    {
        private readonly Queue<object?> _queue;
        private readonly int _capacity;

        public CircularBufferStrategy(int capacity)
        {
            _capacity = capacity;
            _queue = new Queue<object?>();
        }

        public void Add(object item)
        {
            if(_queue.Count > _capacity) _queue.Dequeue();
            _queue.Enqueue(item);
        }

        public IReadOnlyList<object> GetAll()
        {
            var list = new List<object>(_queue);
            _queue.Clear();
            return list;
        }
    }
}
