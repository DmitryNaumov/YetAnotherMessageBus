using System.Collections.Generic;

namespace YAMB.Serialization
{
    public sealed class Message
    {
        public IDictionary<string, string> Headers { get; set; }

        public object Body { get; set; }
    }
}