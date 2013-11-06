using Autofac;
using YAMB.Configuration;

namespace YAMB.Autofac
{
    public static class ConfigurationExtensions
    {
        public static BusConfiguration UseAutofac(this BusConfiguration configuration, ILifetimeScope lifetimeScope, object tag = null)
        {
            return configuration.SubscriptionManager(() => new SubscriptionManager(lifetimeScope, tag));
        }
    }
}