using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedResources.Constants
{
    public static class MessageRouting
    {
        public readonly static string LoggerRoutingKey = "logger-data-routing-key";
        public readonly static string LoggerQueue = "LoggerQueue";

        public readonly static string DataQueue = "DataQueue";
        public readonly static string DataRoutingKey = "data-routing-key";

        public readonly static string ProcessDataQueue = "ProcessQueue";
        public readonly static string ProcessDataRoutingKey = "math-model-routing-key";

        public readonly static string DynamicDataQueue = "DynamicDataQueue";
        public readonly static string DynamicDataRoutingKey = "dynamic-data-routing-key";

        public readonly static string SampleDataQueue = "SampleDataQueue";
        public readonly static string SampleDataRoutingKey = "sample-data-routing-key";

        public readonly static string GeneralDataQueue = "GeneralDataQueue";
        public readonly static string GeneralDataRoutingKey = "general-data-routing-key";

        public readonly static string DMServicePrivateQueue = "DMServicePrivateQueue";
        public readonly static string DMServicePrivateRoutingKey = "dm-service-private-routing-key";

        public readonly static string MMServicePrivateQueue = "MMServicePrivateQueue";
        public readonly static string MMServicePrivateRoutingKey = "mm-service-private-routing-key";

        public readonly static string MathModelOnlinePrivateQueue = "MathModelOnlinePrivateQueue";
        public readonly static string MathModelOnlinePrivateRoutingKey = "math-model-online-private-routing-key";

        public readonly static string SDServicePrivateQueue = "SDServicePrivateQueue";
        public readonly static string SDServicePrivateRoutingKey = "sd-service-private-routing-key";
    }
}
