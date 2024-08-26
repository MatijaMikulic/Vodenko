using MessageModel.Model.DataBlockModel;
using MessageModel.Model.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MessageModel.Utilities
{
    public static class MessageFactory
    {
        public static MessageBase CreateMessage(PlcData plcData)
        {
            if (plcData is L1L2_Sample process)
            {
                return new L2L2_SampleMessage(process, 1);
            }
            else if (plcData is L1L2_Request request)
            {
                return new L2L2_RequestMessage(request, 2);
            }
            else if(plcData is L1L2_ProcessData processData)
            {
                return new L2L2_ProcessData(processData, 1);    
            }
            else if(plcData is L1L2_Alarms alarms)
            {
                return new L2L2_Alarms(alarms, 2);
            }
            else if(plcData is L1L2_ControllerParams controllerParams)
            {
                return new L2L2_ControllerParams(controllerParams, 2);

            }
            else if(plcData is L1L2_SystemStatus systemStatus)
            {
                return new L2L2_SystemStatus(systemStatus, 2); 
            }
            else if(plcData is L1L2_ControlMode controlMode)
            {
                return new L2L2_ControlMode(controlMode, 2);
            }
            else
            {
                throw new ArgumentException("Unsupported PlcData type");
            }
        }
    }
}
