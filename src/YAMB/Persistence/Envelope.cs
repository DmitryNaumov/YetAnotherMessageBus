using System;
using System.IO;

namespace YAMB.Persistence
{
    public sealed class Envelope : IDisposable
    {
        public Guid MessageId { get; set; }
        public DateTime PublishedAt { get; set; }
        public string ContentType { get; set; }
        public Stream MessageStream { get; set; }

        public void Dispose()
        {
            if (MessageStream == null)
                return;

            MessageStream.Dispose();
        }
    }
}