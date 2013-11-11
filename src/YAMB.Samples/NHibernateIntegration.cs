using System;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Context;
using NHibernate.Tool.hbm2ddl;
using YAMB.NHibernateIntegration;

namespace YAMB.Samples
{
    [TestClass]
    public class NHibernateIntegration
    {
        [TestMethod]
        public void RegisterNewUser()
        {
            const string connectionString = @"Data Source=(local); Initial Catalog=YAMB.Samples; Integrated Security=True";

            var sessionFactory = BuildSessionFactory(connectionString);
            var transactionFactory = new NHibernateTransactionFactory(sessionFactory);
            var subscriptionManager = new SubscriptionManager();
            using (var bus = Bus.Configure().ConnectionString(connectionString).SubscriptionManager(subscriptionManager).TransactionFactory(transactionFactory).Build())
            {
                bus.Start();

                var application = new Application(sessionFactory, bus, transactionFactory);
                var task = application.AddUser("dnaumov", "dnaumov@somewhere.com");
                subscriptionManager.Subscribe<UserCreated>(application.Handle);
                Assert.IsTrue(task.Wait(1000));
            }
        }

        private ISessionFactory BuildSessionFactory(string connectionString)
        {
            var configuration = Fluently.Configure()
               .Database(MsSqlConfiguration
                             .MsSql2008
                             .ConnectionString(connectionString))
               .Mappings(m => m.FluentMappings.AddFromAssemblyOf<UserMap>())
               .CurrentSessionContext<CallSessionContext>()
               .BuildConfiguration();

            new SchemaExport(configuration).Execute(true, true, false);

            var sessionFactory = configuration.BuildSessionFactory();
            return sessionFactory;
        }

        internal class Application
        {
            private readonly ISessionFactory _sessionFactory;
            private readonly IBus _bus;
            private readonly NHibernateTransactionFactory _transactionFactory;

            private readonly TaskCompletionSource<int> _tcs = new TaskCompletionSource<int>(); 

            public Application(ISessionFactory sessionFactory, IBus bus, NHibernateTransactionFactory transactionFactory)
            {
                _sessionFactory = sessionFactory;
                _bus = bus;
                _transactionFactory = transactionFactory;
            }

            public Task AddUser(string name, string email)
            {
                using (var session = _sessionFactory.OpenSession())
                using (var transaction = session.BeginTransaction())
                using (_transactionFactory.Bind(session))
                {
                    var user = new User { Name = name, Email = email };
                    session.Save(user);

                    // NOTE: instead of calling SendActivationEmail here, we publish event
                    _bus.Publish(new UserCreated { UserId = user.Id, Name = name, Email = email });

                    transaction.Commit();
                }

                return _tcs.Task;
            }

            public void Handle(UserCreated message)
            {
                var session = _sessionFactory.GetCurrentSession();
                var user = session.Get<User>(message.UserId);

                SendActivationEmail(user);
            }

            private void SendActivationEmail(User user)
            {
                // var mailMessage = new MailMessage(...);
                // SmtpMail.Send(mailMessage);

                Console.WriteLine("Your account has been activated!");

                _tcs.SetResult(0);
            }
        }

        public class User
        {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
            public virtual string Email { get; set; }
        }

        public class UserMap : ClassMap<User>
        {
            public UserMap()
            {
                Id(x => x.Id);
                Map(x => x.Name);
                Map(x => x.Email);
            }
        }

        internal class UserCreated
        {
            public int UserId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
        }
    }
}
