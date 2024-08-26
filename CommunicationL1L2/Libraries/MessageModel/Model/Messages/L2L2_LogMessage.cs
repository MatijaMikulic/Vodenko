using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public enum Severity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Fatal = 3
    }
    public class L2L2_LogMessage : MessageBase
    {
        public string TaskName { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
        public Severity Level { get; set; }

        public L2L2_LogMessage(string taskName, string message, Severity level, byte priority, DateTime? timestamp = null)
            : base(priority, MessageType.LogMessage)
        {
            TaskName = taskName;
            Message = message;
            TimeStamp = timestamp ?? DateTime.Now; ;
            Level = level;
        }

        public override string ToString()
        {
            return $"{TimeStamp} [{Level}]: {Message}\n";
        }
    }

}
