using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace YAMB
{
    public sealed class SubscriptionManager : ISubscriptionManager
    {
        private readonly ConcurrentDictionary<Type, List<Delegate>> _typeMap = new ConcurrentDictionary<Type, List<Delegate>>();

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            var list = _typeMap.GetOrAdd(typeof(T), _ => new List<Delegate>());

            lock (list)
            {
                list.Add(handler);

                return new Disposable(() => Unsubscribe(typeof(T), handler));
            }
        }

        public IEnumerable<Action<TMessage>> GetMessageHandlers<TMessage>()
        {
            List<Delegate> list;
            if (!_typeMap.TryGetValue(typeof(TMessage), out list))
                return Enumerable.Empty<Action<TMessage>>();

            lock (list)
            {
                return list.Cast<Action<TMessage>>().ToArray();
            }
        }

        private void Unsubscribe(Type type, Delegate @delegate)
        {
            List<Delegate> list;
            if (!_typeMap.TryGetValue(type, out list))
                return;

            lock (list)
            {
                list.Remove(@delegate);
            }
        }

        private class Disposable : IDisposable
        {
            private readonly Action _action;

            public Disposable(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }
    }
}
