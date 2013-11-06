namespace YAMB
{
    public interface IBus
    {
        void Publish(object message);

        void PublishNow(params object[] messages);
    }
}
