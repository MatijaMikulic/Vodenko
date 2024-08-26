using RabbitMQ.Client;
using S7.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DataMonitoringService.Constants;
using SharedResources.Constants;
using MessageBroker.Common.Producer;
using PlcCommunication;
using MessageModel.Model.Messages;
using RabbitMQ.Client.Exceptions;
using TaskLog.Contracts;
using System.Reflection;
using PlcCommunication.Model;
using PlcCommunication.Utilities;
using MessageModel.Contracts;

namespace DataMonitoringService.Services
{
    /// <summary>
    /// Service responsible for monitoring data from a PLC and sending updates via RabbitMQ.
    /// </summary>
    public class DMService
    {
        private readonly System.Timers.Timer _timer;                        // Timer to periodically check PLC data
        private readonly IProducerConsumer _producerConsumer;               // RabbitMQ producer-consumer interface
        private readonly ILogger _logger;                                  
        private readonly PlcCommunicationService _plcCommunicationService;  
        private readonly List<DataBlockMetaData> _previousCounterStates;    // Previous states (change & aux counter) of data blocks
        private int _messagesSinceConnection;                               // Counter for messages since the connection
        private const int NumberOfMessagesToSkip = 2;                       // Number of initial messages to skip

        /// <summary>
        /// Initializes a new instance of the <see cref="DMService"/> class.
        /// </summary>
        /// <param name="producer">The RabbitMQ producer-consumer interface.</param>
        /// <param name="plcCommunicationService">The PLC communication service.</param>
        /// <param name="logger">The logger interface.</param>
        public DMService(IProducerConsumer producer, PlcCommunicationService plcCommunicationService, ILogger logger)
        {
            _producerConsumer = producer;
            _plcCommunicationService = plcCommunicationService;
            _logger = logger;
            _messagesSinceConnection = 0;

            // Initialize timer with a 200ms interval
            _timer = new System.Timers.Timer(100) { AutoReset = true };
            _timer.Elapsed += TimerElapsed;

            // Initialize previous counter states with default metadata
            _previousCounterStates = DataBlockMetaDataFactory.CreateDataBlockMetaDataList();
        }

        /// <summary>
        /// Starts the DMService, establishing communication with the PLC and RabbitMQ.
        /// </summary>
        public async Task Start()
        {
            // Start PLC communication and log connection changes
            _plcCommunicationService.Start();
            _plcCommunicationService.PropertyChanged += LogPlcConnectionChange;

            // Open RabbitMQ communication and log connection loss
            await _producerConsumer.OpenCommunication(DataMonitoringServiceInfo.ServiceName);
            _producerConsumer.ConnectionShutdown += LogRabbitMqConnectionLoss;

            // Send start message to RabbitMQ if connected
            if (_producerConsumer.IsConnected())
            {
                _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                    new L2L2_LogMessage(DataMonitoringServiceInfo.ServiceName,
                                        "Data Monitoring Service has started successfully!",
                                        Severity.Info, 1));
            }

            // Start the timer
            _timer.Start();
        }

        /// <summary>
        /// Handles the elapsed event of the timer to check for PLC data updates.
        /// </summary>
        private async void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            // Return if PLC is not connected
            if (!_plcCommunicationService.IsConnected)
            {
                return;
            }

            // Prepare the list which containts the result of reading from plc
            List<DataBlockMetaData> counterStates = new List<DataBlockMetaData>();

            try
            {
                // Read metadata (change counters, aux counters, buffer pointer) from PLC
                //Stopwatch stopwatch = Stopwatch.StartNew();
                counterStates = await _plcCommunicationService.DataAccess.ReadDBMetaDataAsync();
                //stopwatch.Stop();
                //Console.WriteLine($"TimerElapsed function execution time: {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (PlcException ex)
            {
                // Log exception if reading fails
                // LogException(ex);
                return;
            }

            // Process each counter state (for each datablock)
            for (int i = 0; i < counterStates.Count; i++)
            {
                if (HaveCountersChanged(counterStates[i], i))
                {
                    if (_messagesSinceConnection <= NumberOfMessagesToSkip)
                    {
                        _messagesSinceConnection++;
                    }
                    int messagesReady = CalculateMessagesReady(counterStates[i], i);

                    // Log data loss if messages ready exceed buffer size
                    if (messagesReady > counterStates[i].BufferSize)
                    {
                        LogDataLoss(counterStates[i], messagesReady);

                        //Set number of new messages to buffer size because older data was overwritten by new data
                        messagesReady = counterStates[i].BufferSize;
                    }

                    // Process new messages if messages are ready and initial messages are skipped
                    if (messagesReady >= 1 && _messagesSinceConnection > NumberOfMessagesToSkip)
                    {
                        ProcessNewMessages(counterStates[i], messagesReady, i);
                    }
                }
                // Update previous counters with current state
                UpdatePreviousCounters(counterStates[i], i);
            }
        }

        /// <summary>
        /// Checks if the counters have changed since the last read.
        /// </summary>
        private bool HaveCountersChanged(DataBlockMetaData counterState, int i)
        {
            return counterState.ChangeCounter != _previousCounterStates[i].ChangeCounter
                || counterState.AuxiliaryCounter != _previousCounterStates[i].AuxiliaryCounter;
        }

        /// <summary>
        /// Checks if the state of the counters is valid.
        /// </summary>
        private bool IsValidState(DataBlockMetaData counterState, int i)
        {
            bool isChangeCounterValid = counterState.ChangeCounter != _previousCounterStates[i].ChangeCounter;
            bool isAuxiliaryCounterValid = counterState.AuxiliaryCounter == counterState.ChangeCounter;
            bool isBufferPointerValid = counterState.BufferPointer >= 1 && counterState.BufferPointer <= counterState.BufferSize;

            return isChangeCounterValid && isAuxiliaryCounterValid && isBufferPointerValid;
        }

        /// <summary>
        /// Calculates the number of new messages ready to be processed.
        /// </summary>
        private int CalculateMessagesReady(DataBlockMetaData counterState, int i)
        {
            int newMessagesCount = (counterState.AuxiliaryCounter - _previousCounterStates[i].AuxiliaryCounter + ushort.MaxValue + 1) % (ushort.MaxValue + 1);

            return newMessagesCount;
        }

        /// <summary>
        /// Processes the new messages detected in the counter state.
        /// </summary>
        private void ProcessNewMessages(DataBlockMetaData counterState, int messagesReady, int i)
        {
            // Find the value of the buffer pointer at which the oldest message was written and that hasn't been yet processed by service
            int startPointer = (counterState.FindBufferPointer() - messagesReady + counterState.BufferSize) % counterState.BufferSize;

            for (int j = 0; j < messagesReady; j++)
            {
                // Buffer pointer value at which a new message is written
                int pointer = (startPointer + j + counterState.BufferSize) % counterState.BufferSize;
                if (pointer == 0)
                {
                    // Wrap around to BufferSize instead of 0
                    pointer = counterState.BufferSize; 
                }

                // Create a new message to be send via rabbitmq
                L2L2_DataBlockHeader dbheader = new L2L2_DataBlockHeader(counterState.DB, (ushort)pointer, 1);

                try
                {
                    // Send message to RabbitMQ
                    _producerConsumer.SendMessage(MessageRouting.DataRoutingKey, dbheader);
                }
                catch (RabbitMQClientException ex)
                {
                    // Log exception if message sending fails
                    _logger.Log(new L2L2_LogMessage(DataMonitoringServiceInfo.ServiceName, ex.ToString(), Severity.Error, 1));
                }
                // For testing purposes log the current message being sent via rabbitmq
                //_logger.Log(new L2L2_LogMessage(DataMonitoringServiceInfo.ServiceName, 
                //                   $"Sending new message: {counterState.AuxiliaryCounter - (messagesReady - j)}, {pointer}", 
                //                   Severity.Info, 1));
            }

            // Log new data detected
            //if (_producerConsumer.IsConnected())
            //{
            //    _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
            //        new L2L2_LogMessage(DataMonitoringServiceInfo.ServiceName,
            //        $"New data detected from DataBlock: [{counterState.DB}]\t Number of new messages: {messagesReady}",
            //        Severity.Info, 1));
            //}
        }

        /// <summary>
        /// Updates the previous counter states with the current counter state.
        /// </summary>
        private void UpdatePreviousCounters(DataBlockMetaData counterState, int i)
        {
            _previousCounterStates[i].ChangeCounter = counterState.ChangeCounter;
            _previousCounterStates[i].AuxiliaryCounter = counterState.AuxiliaryCounter;
            _previousCounterStates[i].BufferPointer = counterState.BufferPointer;
        }

        /// <summary>
        /// Stops the DMService, closing connections and disposing resources.
        /// </summary>
        public void Stop()
        {
            // Log service stop message if connected
            if (_producerConsumer.IsConnected())
            {
                _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                    new L2L2_LogMessage(DataMonitoringServiceInfo.ServiceName,
                    "Data Monitoring Service has stopped.",
                    Severity.Warning, 1));
            }

            // Dispose services and stop timer
            _plcCommunicationService.Dispose();
            _timer.Stop();
            _timer.Dispose();
            _producerConsumer.Dispose();
        }

        /// <summary>
        /// Logs PLC connection changes.
        /// </summary>
        private void LogPlcConnectionChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_plcCommunicationService.IsConnected))
            {
                bool isConnected = _plcCommunicationService.IsConnected;
                _messagesSinceConnection = 0;
                if (isConnected)
                {
                    LogMessage($"Established connection to plc! Ip address: {_plcCommunicationService.PlcConfiguration.IpAddress}", Severity.Info);
                    MessageBase connectionLossStatus = new L2L2_PlcConnectionStatus(true, 1);
                    _producerConsumer.SendMessage(MessageRouting.GeneralDataRoutingKey, connectionLossStatus);
                }
                else
                {
                    LogMessage($"Lost connection to plc! Ip address: {_plcCommunicationService.PlcConfiguration.IpAddress}", Severity.Warning);
                    MessageBase connectionLossStatus = new L2L2_PlcConnectionStatus(false, 1);
                    _producerConsumer.SendMessage(MessageRouting.GeneralDataRoutingKey, connectionLossStatus);
                }
                Console.WriteLine(isConnected);
            }
        }

        /// <summary>
        /// Logs RabbitMQ connection loss.
        /// </summary>
        private void LogRabbitMqConnectionLoss(object? sender, ShutdownEventArgs e)
        {
            _logger.Log(new L2L2_LogMessage(DataMonitoringServiceInfo.ServiceName, "Lost connection to rabbit mq server!", Severity.Warning, 1));
        }

        /// <summary>
        /// Logs data loss detected in the counter state.
        /// </summary>
        private void LogDataLoss(DataBlockMetaData counterState, int messagesReady)
        {
            int dataLost = messagesReady - counterState.BufferSize;
            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                new L2L2_LogMessage(DataMonitoringServiceInfo.ServiceName,
                $"Data loss detected for [{counterState.DB}]\t Number of data Lost: {dataLost}",
                Severity.Warning, 1));

            _logger.Log(new L2L2_LogMessage(DataMonitoringServiceInfo.ServiceName, $"Data loss: {dataLost}", Severity.Warning, 1));
        }

        /// <summary>
        /// Logs a message with the specified severity.
        /// </summary>
        private void LogMessage(string message, Severity severity)
        {
            if (_producerConsumer.IsConnected())
            {
                _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                    new L2L2_LogMessage(DataMonitoringServiceInfo.ServiceName, message, severity, 1));
            }
            _logger.Log(new L2L2_LogMessage(DataMonitoringServiceInfo.ServiceName, message, Severity.Info, 1));
        }

        /// <summary>
        /// Logs an exception to the console.
        /// </summary>
        private void LogException(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
