using SharedLibrary.Entities;
using DataAccess.Repositories;
using MessageBroker.Common.Producer;
using MessageManagerService.Constants;
using MessageModel.Model.DataBlockModel;
using MessageModel.Model.Messages;
using MessageModel.Utilities;
using PlcCommunication;
using PlcCommunication.Constants;
using S7.Net;
using SharedResources.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MessageManagerService.Services
{
    /// <summary>
    /// Service responsible for managing messages from the database, writing them to the PLC, and logging activities.
    /// </summary>
    public class SService
    {
        private readonly IProducerConsumer _producerConsumer; // RabbitMQ producer-consumer interface
        private readonly PlcCommunicationService _plcCommunicationService; // PLC communication service
        private readonly DatabaseRepositories _databaseRepositories; // Database repositories
        private readonly System.Timers.Timer _timer; // Timer for periodic operations

        /// <summary>
        /// Initializes a new instance of the <see cref="SService"/> class.
        /// </summary>
        /// <param name="producerConsumer">The RabbitMQ producer-consumer interface.</param>
        /// <param name="plcCommunicationService">The PLC communication service.</param>
        /// <param name="databaseRepositories">The database repositories.</param>
        public SService(IProducerConsumer producerConsumer, PlcCommunicationService plcCommunicationService, DatabaseRepositories databaseRepositories)
        {
            _producerConsumer = producerConsumer;
            _plcCommunicationService = plcCommunicationService;
            _databaseRepositories = databaseRepositories;

            _timer = new System.Timers.Timer(1000) { AutoReset = true };
            _timer.Elapsed += TimerElapsed;
        }

        /// <summary>
        /// Handles the timer elapsed event to process new messages from the database.
        /// </summary>
        private async void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            var result = await _databaseRepositories.MessageRepository.GetNewMessages(0);

            foreach (var message in result)
            {
                #region setpoint message
                // Process setpoint message
                if (message.MessageId == 1)
                {
                    L2L1_SetPoint l2L1_SetPoint = new L2L1_SetPoint();
                    Dictionary<string, string> dict = message.GetPayloadDictionary;

                    l2L1_SetPoint.TargetH2Level = GetValueOrDefault(dict, "TargetH2Level", 0f);
                    l2L1_SetPoint.pvInitialValue = GetValueOrDefault(dict, "pvInitialValue", 0f);
                    l2L1_SetPoint.pvFinalValue = GetValueOrDefault(dict, "pvFinalValue", 0f);
                    l2L1_SetPoint.Mode = GetValueOrDefault(dict, "Mode", (ushort)0);

                    message.Status = 1;
                    message.DequeueDT = DateTime.Now;
                    await _databaseRepositories.MessageRepository.UpdateAsync(message);
                    try
                    {
                        UpdatePlcCounters(DataBlockInfo.L2L1_DBIds.SetpointDB);
                        _plcCommunicationService.DataAccess.WriteToDB(l2L1_SetPoint);

                        if (_producerConsumer.IsConnected())
                        {
                            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                                new L2L2_LogMessage(SendInfo.ServiceName,
                                $"Successfully wrote new setpoint message to PLC DB [{DataBlockInfo.L2L1_DBIds.SetpointDB}]: " +
                                $"Message Content: {l2L1_SetPoint.TargetH2Level}, {l2L1_SetPoint.pvInitialValue}, {l2L1_SetPoint.pvFinalValue}, " +
                                $"{l2L1_SetPoint.Mode}",
                                Severity.Info, 1));
                        }
                        Console.WriteLine($"Wrote: {l2L1_SetPoint.pvInitialValue}, {l2L1_SetPoint.pvFinalValue}, {l2L1_SetPoint.Mode}, {l2L1_SetPoint.TargetH2Level}");
                    }
                    catch (PlcException ex)
                    {
                        if (_producerConsumer.IsConnected())
                        {
                            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                                new L2L2_LogMessage(SendInfo.ServiceName,
                                $"Failed to write new setpoint message to PLC DB [{DataBlockInfo.L2L1_DBIds.SetpointDB}]: " +
                                $"Message Content: {l2L1_SetPoint.TargetH2Level}, {l2L1_SetPoint.pvInitialValue}, {l2L1_SetPoint.pvFinalValue}, " +
                                $"{l2L1_SetPoint.Mode}",
                                Severity.Error, 1));
                        }
                    }
                }
                #endregion

                #region controller params message
                // Process controller parameters message
                else if (message.MessageId == 2)
                {
                    L2L1_ControllerParameters l2l1_ControllerParameters = new L2L1_ControllerParameters();
                    Dictionary<string, string> dict = message.GetPayloadDictionary;

                    l2l1_ControllerParameters.Method = GetValueOrDefault(dict, "Method", (ushort)0);
                    l2l1_ControllerParameters.Proportional = GetValueOrDefault(dict, "Proportional", 0f);
                    l2l1_ControllerParameters.Integral = GetValueOrDefault(dict, "Integral", 0f);
                    l2l1_ControllerParameters.Derivative = GetValueOrDefault(dict, "Derivative", 0f);
                    l2l1_ControllerParameters.K1 = GetValueOrDefault(dict, "K1", 0f);
                    l2l1_ControllerParameters.K2 = GetValueOrDefault(dict, "K2", 0f);
                    l2l1_ControllerParameters.K3 = GetValueOrDefault(dict, "K3", 0f);
                    l2l1_ControllerParameters.K4 = GetValueOrDefault(dict, "K4", 0f);

                    message.Status = 1;
                    message.DequeueDT = DateTime.Now;
                    await _databaseRepositories.MessageRepository.UpdateAsync(message);
                    try
                    {
                        UpdatePlcCounters(DataBlockInfo.L2L1_DBIds.CntParamsDB);
                        _plcCommunicationService.DataAccess.WriteToDB(l2l1_ControllerParameters);

                        if (_producerConsumer.IsConnected())
                        {
                            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                                new L2L2_LogMessage(SendInfo.ServiceName,
                                $"Successfully wrote new controller parameters message to PLC DB [{DataBlockInfo.L2L1_DBIds.CntParamsDB}]: " +
                                $"Message Content: {l2l1_ControllerParameters.Method}, {l2l1_ControllerParameters.Proportional}, {l2l1_ControllerParameters.Integral}, " +
                                $"{l2l1_ControllerParameters.Derivative}, {l2l1_ControllerParameters.K1}, {l2l1_ControllerParameters.K2}, {l2l1_ControllerParameters.K3}, " +
                                $"{l2l1_ControllerParameters.K4}",
                                Severity.Info, 1));
                        }

                    }
                    catch (PlcException ex)
                    {
                        if (_producerConsumer.IsConnected())
                        {
                            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                                new L2L2_LogMessage(SendInfo.ServiceName,
                                $"Failed to write new controller parameters message to PLC DB [{DataBlockInfo.L2L1_DBIds.CntParamsDB}]: " +
                                $"Message Content: {l2l1_ControllerParameters.Method}, {l2l1_ControllerParameters.Proportional}, {l2l1_ControllerParameters.Integral}, " +
                                $"{l2l1_ControllerParameters.Derivative}, {l2l1_ControllerParameters.K1}, {l2l1_ControllerParameters.K2}, {l2l1_ControllerParameters.K3}, " +
                                $"{l2l1_ControllerParameters.K4}",
                                Severity.Error, 1));
                        }
                    }
                }
                #endregion

                #region request control message
                // Process request control message
                else if (message.MessageId == 3)
                {
                    L2L1_RequestControl l2L1_RequestControl = new L2L1_RequestControl();
                    Dictionary<string, string> dict = message.GetPayloadDictionary;

                    l2L1_RequestControl.RequestAutoMode = GetValueOrDefault(dict, "RequestAutoMode", (ushort)0);

                    message.Status = 1;
                    message.DequeueDT = DateTime.Now;
                    await _databaseRepositories.MessageRepository.UpdateAsync(message);
                    try
                    {
                        UpdatePlcCounters(DataBlockInfo.L2L1_DBIds.RequestCntDB);
                        _plcCommunicationService.DataAccess.WriteToDB(l2L1_RequestControl);

                        if (_producerConsumer.IsConnected())
                        {
                            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                                new L2L2_LogMessage(SendInfo.ServiceName,
                                $"Successfully wrote new request control message to PLC DB [{DataBlockInfo.L2L1_DBIds.RequestCntDB}]: " +
                                $"Message Content: {l2L1_RequestControl.RequestAutoMode}",
                                Severity.Info, 1));
                        }
                        Console.WriteLine(l2L1_RequestControl.RequestAutoMode);
                    }
                    catch (PlcException ex)
                    {
                        if (_producerConsumer.IsConnected())
                        {
                            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                                new L2L2_LogMessage(SendInfo.ServiceName,
                                $"Failed to write new request control message to PLC DB [{DataBlockInfo.L2L1_DBIds.RequestCntDB}]: " +
                                $"Message Content: {l2L1_RequestControl.RequestAutoMode}",
                                Severity.Error, 1));
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Starts the SService, establishing communication with the PLC and RabbitMQ, and starts the timer.
        /// </summary>
        public async Task Start()
        {
            _plcCommunicationService.Start();

            _timer.Start();
            await _producerConsumer.OpenCommunication();
            if (_producerConsumer.IsConnected())
            {
                _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                new L2L2_LogMessage(SendInfo.ServiceName,
                "Send Service has started.",
                Severity.Info, 1));
            }
        }

        /// <summary>
        /// Stops the SService, sending a stop message to RabbitMQ and disposing resources.
        /// </summary>
        public void Stop()
        {
            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                new L2L2_LogMessage(SendInfo.ServiceName,
                "Send Service has stopped!",
                Severity.Warning, 1));

            _plcCommunicationService.Dispose();
            _producerConsumer.Dispose();
        }

        /// <summary>
        /// Retrieves a value from a dictionary or returns a default value if the key is not found.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="dict">The dictionary to search.</param>
        /// <param name="key">The key to look for.</param>
        /// <param name="defaultValue">The default value to return if the key is not found.</param>
        /// <returns>The value associated with the key or the default value.</returns>
        public T GetValueOrDefault<T>(Dictionary<string, string> dict, string key, T defaultValue = default)
        {
            if (dict.TryGetValue(key, out string value) && TryParse(value, out T result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Tries to parse a string to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to parse to.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <param name="result">The parsed value if successful.</param>
        /// <returns>True if parsing was successful; otherwise, false.</returns>
        public bool TryParse<T>(string value, out T result)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                try
                {
                    result = (T)converter.ConvertFromString(value);
                    return true;
                }
                catch
                {
                    // Handle or log the parsing failure if necessary
                }
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Updates the PLC counters for a specific message ID.
        /// </summary>
        /// <param name="messageId">The message ID to update counters for.</param>
        public void UpdatePlcCounters(int messageId)
        {
            var changeCounter = _plcCommunicationService.DataAccess.ReadChangeCounter(messageId);
            _plcCommunicationService.DataAccess.UpdateChangeCounter(messageId, ++changeCounter);

            var auxCounter = _plcCommunicationService.DataAccess.ReadAuxiliaryCounter(messageId);
            _plcCommunicationService.DataAccess.UpdateAuxCounter(messageId, ++auxCounter);
        }
    }
}
