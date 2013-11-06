using System;
using YAMB.Dispatching;
using YAMB.Persistence;
using YAMB.Routing;
using YAMB.Serialization;
using YAMB.Transaction;

namespace YAMB.Configuration
{
    public sealed class BusConfiguration
    {
        private string _connectionString;
        private Func<ISubscriptionManager> _subscriptionManager = () => new SubscriptionManager();
        
        public BusConfiguration ConnectionString(string connectionString)
        {
            _connectionString = connectionString;

            return this;
        }

        public BusConfiguration SubscriptionManager(ISubscriptionManager subscriptionManager)
        {
            if (subscriptionManager == null)
                throw new ArgumentNullException("subscriptionManager");

            _subscriptionManager = () => subscriptionManager;

            return this;
        }

        public BusConfiguration SubscriptionManager(Func<ISubscriptionManager> subscriptionManager)
        {
            if (subscriptionManager == null)
                throw new ArgumentNullException("subscriptionManager");

            _subscriptionManager = subscriptionManager;

            return this;
        }

        public IBusService Build()
        {
            var transactionFactory = new AdoNetTransactionFactory(_connectionString);
            var queue = new MsSqlMessageQueue("Messages", transactionFactory);
            var serializer = new JsonMessageSerializer();
            var endpoint = new Endpoint(queue, serializer);
            var subscriptionManager = _subscriptionManager();
            var dispatcher = new MessageDispatcher(subscriptionManager);
            return new MessageBus(endpoint, transactionFactory, dispatcher);
        }
    }
}
