using LoggerService.Constants;
using MessageBroker.Common.Producer;
using MessageModel.Model.Messages;
using MessageModel.Utilities;
using Newtonsoft.Json;
using SharedResources.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskLog.Contracts;

namespace LoggerService.Services
{
    /// <summary>
    /// Service responsible for receiving log messages from a message queue and logging them.
    /// </summary>
    public class Service
    {
        private readonly IProducerConsumer _producerConsumer; // RabbitMQ producer-consumer interface
        private readonly ILogger _logger; // Logger interface

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="producerConsumer">The RabbitMQ producer-consumer interface.</param>
        /// <param name="logger">The logger interface.</param>
        public Service(IProducerConsumer producerConsumer, ILogger logger)
        {
            _producerConsumer = producerConsumer;
            _logger = logger;
        }

        /// <summary>
        /// Starts the Service, opening communication with RabbitMQ and reading messages from the queue.
        /// </summary>
        public async Task Start()
        {
            await _producerConsumer.OpenCommunication(LoggerServiceInfo.ServiceName);

            //// Synchronous consumer example (commented out)
            //EventHandler<ReceivedMessageEventArgs> receiverHandler = (sender, args) =>
            //{
            //    var body = MessageDeserializationUtilities.DeserializeMessage(args.body);
            //    if (body is LogMessage logMessage)
            //    {
            //        await _logger.LogAsync(logMessage);
            //    }
            //};
            //while (true)
            //{
            //    _rabbitMqListener.ReadMessageFromQueue(MessageRouting.LoggerQueue, receiverHandler);
            //}

            // Asynchronous consumer
            await _producerConsumer.ReadMessageFromQueueAsync(MessageRouting.LoggerQueue, async (body) =>
            {
                // Deserialize the message body
                var logMessage = MessageDeserializationUtilities.DeserializeMessage(body);
                if (logMessage is L2L2_LogMessage l)
                {
                    await _logger.LogAsync(l); // Log the message asynchronously
                }
            });
        }

        /// <summary>
        /// Stops the Service and disposes of the RabbitMQ producer-consumer.
        /// </summary>
        public void Stop()
        {
            _producerConsumer.Dispose();
        }
    }
}
