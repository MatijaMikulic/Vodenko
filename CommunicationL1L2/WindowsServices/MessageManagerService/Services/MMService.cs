using MessageBroker.Common.Producer;
using MessageManagerService.Constants;
using MessageModel.Model.DataBlockModel;
using MessageModel.Model.Messages;
using MessageModel.Utilities;
using PlcCommunication;
using SharedResources.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageManagerService.Services
{
    /// <summary>
    /// Service responsible for managing messages from PLC and routing them via RabbitMQ.
    /// </summary>
    public class MMService
    {
        private readonly IProducerConsumer _producerConsumer;              // RabbitMQ producer-consumer interface
        private readonly PlcCommunicationService _plcCommunicationService; // PLC communication service

        /// <summary>
        /// Initializes a new instance of the <see cref="MMService"/> class.
        /// </summary>
        /// <param name="producerConsumer">The RabbitMQ producer-consumer interface.</param>
        /// <param name="plcCommunicationService">The PLC communication service.</param>
        public MMService(IProducerConsumer producerConsumer, PlcCommunicationService plcCommunicationService)
        {
            _producerConsumer = producerConsumer;
            _plcCommunicationService = plcCommunicationService;
        }

        /// <summary>
        /// Starts the MMService, establishing communication with the PLC and RabbitMQ.
        /// </summary>
        public async Task Start()
        {
            // Start PLC communication
            _plcCommunicationService.Start();

            // Open RabbitMQ communication
            await _producerConsumer.OpenCommunication(MessageManagerInfo.ServiceName);
            _producerConsumer.PurgeQueue(MessageRouting.DataQueue);

            // Send start message to RabbitMQ
            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                new L2L2_LogMessage(
                    MessageManagerInfo.ServiceName,
                    "Message Manager Service has started.",
                    Severity.Info,
                    1));

            // Read messages from the data queue asynchronously
            await _producerConsumer.ReadMessageFromQueueAsync(MessageRouting.DataQueue, async (body) =>
            {
                var message = MessageDeserializationUtilities.DeserializeMessage(body);

                if (message is L2L2_DataBlockHeader m)
                {
                    // Reads content of buffer element (stucture)
                    var data = _plcCommunicationService.DataAccess.ReadDBContent(m.DB, m.BufferPointer);

                    await HandleDataAsync(data);
                }
            });
        }

        /// <summary>
        /// Handles the data read from the PLC by routing it to the appropriate RabbitMQ queue.
        /// </summary>
        private async Task HandleDataAsync(object data)
        {
            switch (data)
            {
                case L1L2_ProcessData processData:
                    await SendMessageAsync(MessageFactory.CreateMessage(processData), MessageRouting.ProcessDataRoutingKey);
                    break;
                case L1L2_Alarms alarms:
                    await SendMessageAsync(MessageFactory.CreateMessage(alarms), MessageRouting.SampleDataRoutingKey);
                    break;
                case L1L2_ControllerParams controllerParams:
                    await SendMessageAsync(MessageFactory.CreateMessage(controllerParams), MessageRouting.GeneralDataRoutingKey);
                    break;
                case L1L2_ControlMode controlMode:
                    await SendMessageAsync(MessageFactory.CreateMessage(controlMode), MessageRouting.GeneralDataRoutingKey);
                    break;
                case L1L2_SystemStatus systemStatus:
                    await SendMessageAsync(MessageFactory.CreateMessage(systemStatus), MessageRouting.GeneralDataRoutingKey);
                    break;
            }
        }

        /// <summary>
        /// Sends a message to the RabbitMQ queue with the specified routing key.
        /// </summary>
        private async Task SendMessageAsync<T>(T message, string routingKey)
        {
            if (message is L2L2_LogMessage logMessage)
            {
                _producerConsumer.SendMessage(routingKey, logMessage);
            }
            else if (message is L2L2_ProcessData processData)
            {
                processData.ProcessData.InletFlow = processData.ProcessData.InletFlow * (1000 / 60.0f);
                processData.ProcessData.OutletFlow = processData.ProcessData.OutletFlow * (1000 / 60.0f);
                //Console.WriteLine($"Received new message: {processData.ProcessData.Sample}, {processData.ProcessData.WaterLevelTank2}, {processData.ProcessData.WaterLevelTank1}, {processData.ProcessData.IsPumpActive}, {processData.ProcessData.TargetWaterLevelTank2}: \t [{processData.ProcessData.GetDateTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}] ");
               
                _producerConsumer.SendMessage(routingKey, processData);
            }
            else if (message is L2L2_Alarms alarms)
            {
                _producerConsumer.SendMessage(routingKey, alarms);
            }
            else if (message is L2L2_ControllerParams controllerParams)
            {
                //Console.WriteLine($"{controllerParams.ControllerParams.Proportional}, " +
                //    $"{controllerParams.ControllerParams.Integral}," +
                //    $"{controllerParams.ControllerParams.Derivative}, " +
                //    $"{controllerParams.ControllerParams.Method}, {controllerParams.ControllerParams.K1}" +
                //    $"{controllerParams.ControllerParams.K2}, {controllerParams.ControllerParams.K3}, {controllerParams.ControllerParams.K4}");
                _producerConsumer.SendMessage(routingKey, controllerParams);
            }
            else if (message is L2L2_ControlMode controlMode)
            {
                //Console.WriteLine($"{controlMode.ControlMode.ControlMode}");
                _producerConsumer.SendMessage(routingKey, controlMode);
            }
            else if (message is L2L2_SystemStatus systemStatus)
            {
                //Console.WriteLine
                //   ($"{systemStatus.SystemStatus.IsProportionalValveActive}, " +
                //    $"{systemStatus.SystemStatus.IsPumpActive}, " +
                //    $"{systemStatus.SystemStatus.IsLowLevelSwitchActive}, " +
                //    $"{systemStatus.SystemStatus.IsTank1CrtiticalLevel}," +
                //    $" {systemStatus.SystemStatus.IsTank2CrtiticalLevel}");
                _producerConsumer.SendMessage(routingKey, systemStatus);
            }
        }

        /// <summary>
        /// Stops the MMService and disposes of the RabbitMQ producer-consumer and PLC communication service.
        /// </summary>
        public void Stop()
        {
            // Send stop message to RabbitMQ
            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                new L2L2_LogMessage(MessageManagerInfo.ServiceName,
                "Message Manager Service has exited!",
                Severity.Warning, 1));

            // Dispose services
            _plcCommunicationService.Dispose();
            _producerConsumer.Dispose();
        }
    }
}
