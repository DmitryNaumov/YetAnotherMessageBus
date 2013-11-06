namespace YAMB.Autofac
{
    public interface IMessageHandler<TMessage>
    {
        void Handle(TMessage message);
    }
}