using V3.S7Plc.Communication.Interfaces;

namespace V3.S7Plc.Communication.Buffer
{
    /// <summary>
    /// Single buffer: only keeps the most recent value.
    /// </summary>
    public class SingleBufferStrategy : IBufferStrategy
    {
        private object? _latest;
        public void Add(object item)
        {
            _latest = item;
        }

        public IReadOnlyList<object> GetAll()
        {
            if( _latest == null ) return new List<object>();
            var list = new List<object> { _latest };
            _latest = null;
            return list;
        }
    }
}
