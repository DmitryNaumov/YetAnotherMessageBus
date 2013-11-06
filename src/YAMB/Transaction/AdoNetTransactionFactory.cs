using System;
using System.Data;
using System.Data.SqlClient;

namespace YAMB.Transaction
{
    internal sealed class AdoNetTransactionFactory : ITransactionFactory
    {
        [ThreadStatic]
        private static AdoNetTransaction _currentTransaction;

        private readonly string _connectionString;

        public AdoNetTransactionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDisposable Bind(SqlTransaction transaction)
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException();

            return _currentTransaction = new AdoNetTransaction(this, transaction);
        }

        public ITransaction CreateTransaction()
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException();

            return _currentTransaction = new AdoNetTransaction(this);
        }

        public void EnlistInTransaction(IDbCommand command)
        {
            var transaction = _currentTransaction;
            if (transaction == null)
                throw new InvalidOperationException();

            transaction.Enlist(command);
        }

        private string ConnectionString
        {
            get { return _connectionString; }
        }

        private void AfterTransactionCompletion(AdoNetTransaction transaction)
        {
            if (_currentTransaction != transaction)
                throw new InvalidProgramException();

            _currentTransaction = null;
        }

        private class AdoNetTransaction : ITransaction
        {
            private readonly AdoNetTransactionFactory _transactionFactory;

            private SqlConnection _connection;
            private SqlTransaction _transaction;
            private readonly bool _external;

            public AdoNetTransaction(AdoNetTransactionFactory transactionFactory, SqlTransaction transaction = null)
            {
                _transactionFactory = transactionFactory;

                if (transaction == null)
                {
                    var connection = new SqlConnection(transactionFactory.ConnectionString);
                    try
                    {
                        connection.Open();
                        _transaction = connection.BeginTransaction();
                        _connection = connection;
                    }
                    catch
                    {
                        connection.Dispose();

                        throw;
                    }
                }
                else
                {
                    _transaction = transaction;
                    _connection = transaction.Connection;
                    _external = true;
                }
            }

            public void Enlist(IDbCommand command)
            {
                command.Connection = _transaction.Connection;
                command.Transaction = _transaction;
            }

            public void Commit()
            {
                if (_external)
                    throw new InvalidOperationException();

                _transaction.Commit();
            }

            public void Dispose()
            {
                if (_transaction == null)
                    return;

                if (!_external)
                {
                    _transaction.Dispose();
                    _connection.Dispose();
                }

                _transaction = null;
                _connection = null;

                _transactionFactory.AfterTransactionCompletion(this);
            }
        }
    }
}