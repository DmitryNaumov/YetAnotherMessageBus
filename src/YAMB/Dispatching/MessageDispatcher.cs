using System;
using System.Collections.Concurrent;

namespace YAMB.Dispatching
{
    internal sealed class MessageDispatcher : IMessageDispatcher
    {
        private readonly ISubscriptionManager _subscriptionManager;

        private readonly ConcurrentDictionary<Type, Type> _typeCache = new ConcurrentDictionary<Type, Type>();

        public MessageDispatcher(ISubscriptionManager subscriptionManager)
        {
            _subscriptionManager = subscriptionManager;
        }

        public void Dispatch(object message)
        {
            var messageType = message.GetType();
            var callerType = _typeCache.GetOrAdd(messageType, type => typeof(Caller<>).MakeGenericType(type));

            Activator.CreateInstance(callerType, _subscriptionManager, message);
        }

        private class Caller<TMessage>
        {
            public Caller(ISubscriptionManager subscriptionManager, TMessage message)
            {
                foreach (var handler in subscriptionManager.GetMessageHandlers<TMessage>())
                {
                    handler(message);
                }
            }
        }
    }
}