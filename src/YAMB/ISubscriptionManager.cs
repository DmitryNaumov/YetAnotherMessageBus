using System;
using System.Collections.Generic;

namespace YAMB
{
    public interface ISubscriptionManager
    {
        IEnumerable<Action<TMessage>> GetMessageHandlers<TMessage>();
    }
}