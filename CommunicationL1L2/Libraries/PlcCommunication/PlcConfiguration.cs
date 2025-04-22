namespace PlcCommunication
{
    public class PlcConfiguration
    {
        public required string CpuType { get; init; } 
        public required string IpAddress { get; init; } 
        public required short Rack { get; init; }
        public required short Slot { get; init; }
    }
}
