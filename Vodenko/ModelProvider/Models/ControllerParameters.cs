namespace ModelProvider.Models
{
    public class ControllerParameters
    {
        public ushort Method { get; set; }
        public float Proportional { get; set; }
        public float Integral { get; set; }
        public float Derivative { get; set; }
        public float K1 { get; set; }
        public float K2 { get; set; }
        public float K3 { get; set; }
        public float K4 { get; set; }

        public ControllerParameters(ushort method, float proportional, float integral, float derivative, float k1, float k2, float k3, float k4)
        {
            Proportional = proportional;
            Integral = integral;
            Derivative = derivative;
            Method = method;
            K1 = k1;
            K2 = k2;
            K3 = k3;
            K4 = k4;
        }

        public ControllerParameters() { }
    }
}
