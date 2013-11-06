using System;
using System.IO;
using YAMB.Persistence;
using YAMB.Serialization;

namespace YAMB.Routing
{
    internal sealed class Endpoint : IEndpoint
    {
        private readonly IMessageQueue _queue;
        private readonly IMessageSerializer _serializer;

        public Endpoint(IMessageQueue queue, IMessageSerializer serializer)
        {
            if (queue == null)
                throw new ArgumentNullException("queue");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            _queue = queue;
            _serializer = serializer;
        }

        public void Send(object message)
        {
            var envelope = new Envelope
            {
                MessageId = Guid.NewGuid(),
                PublishedAt = DateTime.UtcNow,

                MessageStream = new MemoryStream()
            };

            try
            {
                _serializer.Serialize(envelope.MessageStream, new Message {Body = message});
                envelope.MessageStream.Position = 0;
            }
            catch
            {
                envelope.Dispose();

                throw;
            }

            _queue.Send(envelope);
        }

        public object Receive(Action receiveCallback)
        {
            using (var envelope = _queue.Receive())
            {
                if (envelope == null)
                    return null;

                if (receiveCallback != null)
                    receiveCallback();

                // TODO: choose serializer based on ContentType
                var message = _serializer.Deserialize(envelope.MessageStream);
                return message.Body;
            }
        }
    }
}