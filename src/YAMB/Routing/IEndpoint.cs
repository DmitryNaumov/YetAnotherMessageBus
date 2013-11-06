using System;

namespace YAMB.Routing
{
    public interface IEndpoint
    {
        void Send(object message);
        object Receive(Action receiveCallback);
    }
}