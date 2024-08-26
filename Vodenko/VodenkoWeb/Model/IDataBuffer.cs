namespace VodenkoWeb.Model
{
    public interface IDataBuffer<T> where T : class
    {
        public void Add(T data);
        public List<T> GetLatestData();
        
    }
}
