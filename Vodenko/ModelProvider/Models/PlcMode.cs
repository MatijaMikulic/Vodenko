namespace ModelProvider.Models
{
    public class PlcMode
    {
        
        public ushort ControlMode { get; set; }

        public string Description { get; set; }

        public PlcMode(ushort controlMode, string description)
        {
            ControlMode = controlMode;
            Description = description;
        }

        public PlcMode() { }

    }
}
