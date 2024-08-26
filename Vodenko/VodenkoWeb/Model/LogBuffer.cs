namespace VodenkoWeb.Model
{
    public class LogBuffer : Queue<string>
    {
        public int? MaxCapacity { get; }

        public LogBuffer(int capacity)
        {
            MaxCapacity = capacity;
        }

        public int TotalItemsAddedCount { get; private set; }

        public new void Enqueue(string log)
        {
            if (Count == MaxCapacity)
            {
                Dequeue();
            }
            base.Enqueue(log);
            TotalItemsAddedCount++;
        }

        public List<string> GetLatestData()
        {
            return this.ToList(); // Convert queue to a list
        }
    }
}
