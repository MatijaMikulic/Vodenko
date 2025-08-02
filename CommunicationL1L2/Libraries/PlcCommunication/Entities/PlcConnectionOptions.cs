namespace V3.S7Plc.Communication.Entities
{
    /// <summary>Holds IP/rack/slot/CPU for S7-PLC.</summary>
    public class PlcConnectionOptions
    {
        public required string CpuType { get; init; }
        public required string IpAddress { get; init; }
        public required short Rack { get; init; }
        public required short Slot { get; init; }
    }
}
