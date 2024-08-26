using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathModelOnline.Constants
{
    public static class ModelParameters
    {
        public static Dictionary<string, double> variables = new Dictionary<string, double>
        {
            { "g", 981 },
            { "K12A", 0.7132 },
            { "K12B", 1.2516 },
            { "K1d", 0.1351 },
            { "K2d", 0.1351 },
            { "Ki", 0.5913 },
            { "Tv", 1.74 },
            { "Kv", 3.2519 },
            { "A1", 450 },
            { "x", -12}
        };

        public static Dictionary<string, double> initialValues = new Dictionary<string, double>
        {
            { "h20", 4.9 },
            { "h10", 6.18 },
            { "qu0", 81 }
        };

        public static Dictionary<string, double> inputs = new Dictionary<string, double>
        {
            {"xv0", 25}
        };


        public static string[,] A = {
            { "-1.0 / Tv", "0", "0" },
            { "1.0 / A1", "-((K12A + K12B) * g / Sqrt(2 * g * (h10 - h20)) + K1d * g / Sqrt(2 * g * h10)) / A1", "(K12A + K12B) * g / Sqrt(2 * g * (h10 - h20)) / A1" },
            { "0", "(K12A + K12B) * g / Sqrt(2 * g * (h10 - h20)) / A1", "-((K12A + K12B) * g / Sqrt(2 * g * (h10 - h20)) + (K2d) * g / Sqrt(2 * g * h20) + Ki * g / Sqrt(2 * g * (h20-x))) / A1" }
        };

        public static string[,] B = {
            { "Kv / Tv" },
            { "0" },
            { "0" }
        };

        public static string[,] C = {
            { "0", "0", "1" }
        };
    }
}
