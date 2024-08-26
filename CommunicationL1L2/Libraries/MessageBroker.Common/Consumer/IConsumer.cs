using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common.Consumer
{
    /// <summary>
    /// Defines the interface for a message consumer, providing methods for reading messages from a queue.
    /// </summary>
    //public interface IConsumer:IDisposable
    //{
    //    /// <summary>
    //    /// Reads a message from the specified queue and subscribes to the event when a message is received.
    //    /// </summary>
    //    /// <param name="queue">The name of the queue from which to read the message.</param>
    //    /// <param name="onMessageReceivedEvent">The event handler to be executed when a message is received.</param>
    //    public void ReadMessageFromQueue(string queue,EventHandler<ReceivedMessageEventArgs> onMessageReceivedEvent);

    //    /// <summary>
    //    /// Reads a message from the specified queue asynchronously and invokes the provided asynchronous callback.
    //    /// </summary>
    //    /// <param name="queue">The name of the queue from which to read the message.</param>
    //    /// <param name="onMessageReceived">The asynchronous callback to be invoked when a message is received.</param>
    //    /// <returns>A Task representing the asynchronous operation.</returns>
    //    Task ReadMessageFromQueueAsync(string queue, Func<byte[], Task> onMessageReceived);

    //    /// <summary>
    //    /// Opens the communication channel for the consumer.
    //    /// </summary
    //    public Task OpenCommunication();

    //    /// <summary>
    //    /// Checks whether the consumer is connected to the message queue.
    //    /// </summary>
    //    /// <returns>True if connected; otherwise, false.</returns>
    //    public bool  IsConnected();

    //    /// <summary>
    //    /// Event raised when the connection to the message queue is shut down unexpectedly.
    //    /// The event handler receives arguments of type ShutdownEventArgs which contain 
    //    /// details about the shutdown reason.
    //    /// </summary>
    //    public event EventHandler<ShutdownEventArgs>? ConnectionShutdown;

    //}
}
