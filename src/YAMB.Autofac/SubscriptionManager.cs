using System;
using System.Collections.Generic;
using Autofac;

namespace YAMB.Autofac
{
    internal sealed class SubscriptionManager : ISubscriptionManager
    {
        private readonly ILifetimeScope _container;
        private readonly object _tag;

        public SubscriptionManager(ILifetimeScope container, object tag = null)
        {
            _container = container;
            _tag = tag;
        }

        public IEnumerable<Action<TMessage>> GetMessageHandlers<TMessage>()
        {
            using (var innerScope = BeginLifetimeScope())
            {
                var handlers = innerScope.Resolve<IEnumerable<IMessageHandler<TMessage>>>();
                foreach (var handler in handlers)
                {
                    yield return handler.Handle;
                }
            }
        }

        private ILifetimeScope BeginLifetimeScope()
        {
            return _tag != null ? _container.BeginLifetimeScope(_tag) : _container.BeginLifetimeScope();
        }
    }
}
