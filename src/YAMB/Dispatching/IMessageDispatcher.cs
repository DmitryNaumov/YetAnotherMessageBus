namespace YAMB.Dispatching
{
    public interface IMessageDispatcher
    {
        void Dispatch(object message);
    }
}