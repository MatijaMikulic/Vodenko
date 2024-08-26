namespace ModelProvider.Models
{
    public class PlcConnectionStatus
    {
        public bool IsConnected { get; set; }

        public PlcConnectionStatus(bool isConnected)
        {
            IsConnected = isConnected;
        }

        public PlcConnectionStatus()
        {
            IsConnected = false;
        }

    }
}
