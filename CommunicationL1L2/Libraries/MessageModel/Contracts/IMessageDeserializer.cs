using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageModel.Model.Messages;

namespace MessageModel.Contracts
{
    /// <summary>
    /// Interface for message deserialization.
    /// </summary>
    public interface IMessageDeserializer
    {
        /// <summary>
        /// Deserializes the provided byte array into a message object.
        /// </summary>
        /// <param name="body">The byte array representing the serialized message.</param>
        /// <returns>An instance of the MessageBase class representing the deserialized message.</returns>
        MessageBase Deserialize(byte[] body);
    }
}
