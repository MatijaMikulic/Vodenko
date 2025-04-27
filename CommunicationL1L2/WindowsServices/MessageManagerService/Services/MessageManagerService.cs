using MessageManagerService.Constants;
namespace MessageManagerService.Services
{
    using MessageBroker.Common.Producer;
    using MessageModel.Model.DataBlockModel;
    using MessageModel.Model.Messages;
    using MessageModel.Utilities;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using PlcCommunication.Interfaces;
    using SharedResources.Constants;

    /// <summary>
    /// Service responsible for managing messages from PLC and routing them via RabbitMQ.
    /// </summary>
    public sealed class MessageManagerService: BackgroundService
    {
        private readonly IProducerConsumer _producerConsumer;            
        private readonly IConnectionManager _connectionManager;
        private readonly IPlcDataAccess _dataAccess;
        private readonly ILogger<MessageManagerService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageManagerService"/> class.
        /// </summary>
        /// <param name="producerConsumer">The RabbitMQ producer-consumer interface.</param>
        /// <param name="connectionManager">The plc communication interface.</param>
        /// <param name="plcDataAccess">The data access interface.</param>
        public MessageManagerService(
            IProducerConsumer producerConsumer, 
            IConnectionManager connectionManager, 
            IPlcDataAccess plcDataAccess,
            ILogger<MessageManagerService> logger)
        {
            this._producerConsumer = producerConsumer;
            this._connectionManager = connectionManager;
            this._dataAccess = plcDataAccess;
            this._logger = logger;    
        }

        /// <summary>
        /// Starts the MMService, establishing communication with the PLC and RabbitMQ.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // 1) Start PLC communication
            try
            {
                _connectionManager.Open();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(new L2L2_LogMessage(
                    MessageManagerInfo.ServiceName,
                    $"Initial PLC open failed: {ex.Message}",
                    Severity.Fatal, 1).ToString());
            }
            _connectionManager.ConnectionStatusChanged += OnPlcConnectionChanged;

            // 2) Open RabbitMQ communication
            await _producerConsumer.OpenCommunication(MessageManagerInfo.ServiceName);
            _producerConsumer.PurgeQueue(MessageRouting.DataQueue);

            // 3) Send start message to RabbitMQ
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
                    var data = _dataAccess.ReadContent(m.DB, m.BufferPointer);
                    await HandleDataAsync(data, cancellationToken);
                }
            });

        }

        /// <summary>
        /// Stops the MMService and disposes of the RabbitMQ producer-consumer and PLC communication service.
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Send stop message to RabbitMQ
            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                new L2L2_LogMessage(MessageManagerInfo.ServiceName,
                "Message Manager Service has exited!",
                Severity.Warning, 1));

            // Dispose services
            _connectionManager.Close();
            _producerConsumer.Dispose();

            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// Handles the data read from the PLC by routing it to the appropriate RabbitMQ queue.
        /// </summary>
        private async Task HandleDataAsync(object data, CancellationToken cancellationToken)
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
               
                _producerConsumer.SendMessage(routingKey, processData);
            }
            else if (message is L2L2_Alarms alarms)
            {
                _producerConsumer.SendMessage(routingKey, alarms);
            }
            else if (message is L2L2_ControllerParams controllerParams)
            {
                _producerConsumer.SendMessage(routingKey, controllerParams);
            }
            else if (message is L2L2_ControlMode controlMode)
            {
                _producerConsumer.SendMessage(routingKey, controlMode);
            }
            else if (message is L2L2_SystemStatus systemStatus)
            {
                _producerConsumer.SendMessage(routingKey, systemStatus);
            }
        }


        private void OnPlcConnectionChanged(object? sender, bool isUp)
        {
            var sev = isUp ? Severity.Info : Severity.Warning;
            var text = isUp ? "PLC connected" : "PLC disconnected";

            if (_producerConsumer != null && _producerConsumer.IsConnected())
            {
                _producerConsumer.SendMessage(
                    MessageRouting.GeneralDataRoutingKey,
                    new L2L2_PlcConnectionStatus(isUp, 1));
            }
            _logger.LogInformation(new L2L2_LogMessage(
                MessageManagerInfo.ServiceName,
                text, sev, 1).ToString());
        }
    }
}
