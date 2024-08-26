namespace ModelProvider.Models
{
    public class PlantStatus
    {
        public bool PumpActive { get; set; }
        public bool LevelSwitch { get; set; }
        public bool ValveOperatingRange { get; set; }
        public bool Tank1Level { get; set; }
        public bool Tank2Level { get; set; }
        public bool SystemReady { get; set; }

        public PlantStatus(bool pumpActive, bool levelSwitch, bool valveOperatingRange, bool tank1Level, bool tank2Level, bool systemReady)
        {
            PumpActive = pumpActive;
            LevelSwitch = levelSwitch;
            ValveOperatingRange = valveOperatingRange;
            Tank1Level = tank1Level;
            Tank2Level = tank2Level;
            SystemReady = systemReady;
        }
        public PlantStatus() { }

    }
}
