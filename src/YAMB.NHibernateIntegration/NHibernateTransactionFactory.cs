using System;
using System.Data;
using NHibernate;
using NHibernate.Context;
using YAMB.Transaction;
using ITransaction = YAMB.Transaction.ITransaction;

namespace YAMB.NHibernateIntegration
{
    public sealed class NHibernateTransactionFactory : ITransactionFactory
    {
        private readonly NHibernate.ISessionFactory _sessionFactory;

        public NHibernateTransactionFactory(NHibernate.ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public IDisposable Bind(ISession session)
        {
            return new Transaction(session);
        }

        public ITransaction CreateTransaction()
        {
            return new Transaction(_sessionFactory);
        }

        public void EnlistInTransaction(IDbCommand command)
        {
            var session = _sessionFactory.GetCurrentSession();
            if (session == null || session.Transaction == null || session.Transaction.IsActive == false)
                throw new InvalidOperationException();

            session.Transaction.Enlist(command);
            command.Connection = session.Connection;
        }

        private class Transaction : ITransaction
        {
            private readonly NHibernate.ISession _session;
            private readonly NHibernate.ITransaction _transaction;
            private readonly bool _external;

            public Transaction(NHibernate.ISessionFactory sessionFactory)
            {
                _session = sessionFactory.OpenSession();
                _transaction = _session.BeginTransaction();

                CurrentSessionContext.Bind(_session);
            }

            public Transaction(ISession session)
            {
                if (session.Transaction == null || session.Transaction.IsActive == false)
                    throw new ArgumentException("session");

                _session = session;
                _transaction = session.Transaction;
                _external = true;

                CurrentSessionContext.Bind(_session);
            }

            public void Commit()
            {
                if (_external)
                    throw new InvalidOperationException();

                if (_session.Transaction != _transaction)
                    throw new InvalidProgramException();

                _transaction.Commit();
                _session.Close();
            }

            public void Dispose()
            {
                if (!_external)
                {
                    _transaction.Dispose();
                    _session.Dispose();
                }

                CurrentSessionContext.Unbind(_session.SessionFactory);
            }
        }
    }
}
