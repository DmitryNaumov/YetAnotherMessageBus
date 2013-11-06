namespace YAMB.Persistence
{
    public interface IMessageQueue
    {
        void Send(Envelope envelope);
        Envelope Receive();
    }
}