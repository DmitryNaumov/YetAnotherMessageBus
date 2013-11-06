using System;
using System.Data;
using System.Data.SqlClient;
using YAMB.Transaction;

namespace YAMB.Persistence
{
    internal sealed class MsSqlMessageQueue : IMessageQueue
    {
        private readonly ITransactionFactory _transactionFactory;

        private readonly string _insertCommand;
        private readonly string _deleteCommand;

        public MsSqlMessageQueue(string tableName, ITransactionFactory transactionFactory)
        {
            _transactionFactory = transactionFactory;

            _insertCommand = string.Format(@"INSERT INTO {0} (MessageId, ContentType, PublishedAt, Message) VALUES (@MessageId, @ContentType, @PublishedAt, @Message)", tableName);
            _deleteCommand = string.Format(@"DELETE TOP(1) FROM {0} WITH (ROWLOCK, READPAST) OUTPUT DELETED.MessageId, DELETED.ContentType, DELETED.PublishedAt, DELETED.Message", tableName);
        }

        public void Send(Envelope envelope)
        {
            using (envelope)
            {
                var command = new SqlCommand(_insertCommand);

                command.Parameters.Add("MessageId", SqlDbType.UniqueIdentifier).Value = envelope.MessageId;
                command.Parameters.Add("ContentType", SqlDbType.NVarChar).Value =
                    string.IsNullOrWhiteSpace(envelope.ContentType) ? DBNull.Value : (object) envelope.ContentType;
                command.Parameters.Add("PublishedAt", SqlDbType.DateTime2).Value = envelope.PublishedAt;
                command.Parameters.Add("Message", SqlDbType.VarBinary).Value = envelope.MessageStream;

                EnlistCommand(command);

                command.ExecuteNonQuery();
            }
        }

        public Envelope Receive()
        {
            var command = new SqlCommand(_deleteCommand);

            EnlistCommand(command);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var messageId = reader.GetGuid(0);
                    var contentType = reader.IsDBNull(1) ? null : reader.GetString(1);
                    var publishedAt = reader.GetDateTime(2);
                    var messageStream = reader.GetStream(3);

                    return new Envelope
                    {
                        MessageId = messageId,
                        ContentType = contentType,
                        PublishedAt = publishedAt,
                        MessageStream = messageStream
                    };
                }
            }

            return null;
        }

        private void EnlistCommand(IDbCommand command)
        {
            _transactionFactory.EnlistInTransaction(command);
        }
    }
}
