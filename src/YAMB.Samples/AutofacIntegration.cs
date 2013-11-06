using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YAMB.Autofac;
using YAMB.Configuration;
using YAMB.Samples.Messages;

namespace YAMB.Samples
{
    [TestClass]
    public class AutofacIntegration
    {
        [TestMethod]
        public void DispatchMessageToRegisteredHandler()
        {
            const string connectionString = @"Data Source=(local); Initial Catalog=YAMB.Samples; Integrated Security=True";
            
            new SchemaExporter(connectionString).Create();

            var containerBuilder = new ContainerBuilder();
            containerBuilder
                .Register(c => Bus
                    .Configure()
                    .ConnectionString(connectionString)
                    .UseAutofac(c.Resolve<ILifetimeScope>())
                    .Build())
                .SingleInstance();

            containerBuilder
                .RegisterType<Application>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            using (var container = containerBuilder.Build())
            {
                var task = container.Resolve<Application>().Run();
                Assert.IsTrue(task.Wait(1000));
            }
        }

        private class Application : IMessageHandler<PingMessage>
        {
            private readonly IBusService _bus;
            private readonly TaskCompletionSource<int> _tcs = new TaskCompletionSource<int>();

            public Application(IBusService bus)
            {
                _bus = bus;
            }

            public Task Run()
            {
                _bus.Start();

                _bus.PublishNow(new PingMessage {Text = "Hello, %username%!"});

                return _tcs.Task;
            }

            public void Handle(PingMessage message)
            {
                _tcs.SetResult(0);
            }
        }
    }
}
