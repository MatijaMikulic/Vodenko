using MessageModel.Model.Messages;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common.Producer
{
    /// <summary>
    /// Defines the interface for a message producer, providing methods for sending messages to a message queue.
    /// </summary>
    public interface IProducerConsumer:IDisposable
    {
        /// <summary>
        /// Sends a message with the specified routing key.
        /// </summary>
        /// <param name="routingKey">The routing key for the message.</param>
        /// <param name="message">The message to be sent.</param>
        public void SendMessage(string routingKey, MessageBase message);

        /// <summary>
        /// Reads a message from the specified queue and subscribes to the event when a message is received.
        /// </summary>
        /// <param name="queue">The name of the queue from which to read the message.</param>
        /// <param name="onMessageReceivedEvent">The event handler to be executed when a message is received.</param>
        public void ReadMessageFromQueue(string queue, EventHandler<ReceivedMessageEventArgs> onMessageReceivedEvent);

        /// <summary>
        /// Reads a message from the specified queue asynchronously and invokes the provided asynchronous callback.
        /// </summary>
        /// <param name="queue">The name of the queue from which to read the message.</param>
        /// <param name="onMessageReceived">The asynchronous callback to be invoked when a message is received.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task ReadMessageFromQueueAsync(string queue, Func<byte[], Task> onMessageReceived);

        // <summary>
        /// Opens the communication channel for the producer.
        /// </summary>
        public Task OpenCommunication();

        /// <summary>
        /// Checks whether the producer is connected to the message queue.
        /// </summary>
        /// <returns>True if connected; otherwise, false.</returns>
        public bool IsConnected();

        /// <summary>
        /// Event raised when the connection to the message queue is shut down unexpectedly.
        /// The event handler receives arguments of type ShutdownEventArgs which contain 
        /// details about the shutdown reason.
        /// </summary>
        public event EventHandler<ShutdownEventArgs>? ConnectionShutdown;

    }
}
