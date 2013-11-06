using System.IO;

namespace YAMB.Serialization
{
    public interface IMessageSerializer
    {
        void Serialize(Stream stream, Message message);
        Message Deserialize(Stream stream);
    }
}