using System.Data.SqlClient;

namespace YAMB.Configuration
{
    public sealed class SchemaExporter
    {
        private readonly string _connectionString;

        private const string DropCommand = @"
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Messages]') AND type in (N'U'))
DROP TABLE [dbo].[Messages]
";

        private const string CreateCommand = @"
CREATE TABLE [dbo].[Messages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
    [MessageId] [uniqueidentifier] NOT NULL,
    [ContentType] [nvarchar](500) NULL,
	[PublishedAt] [datetime2](7) NOT NULL,
	[Message] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_Messages] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
))
";

        public SchemaExporter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Create()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var command = new SqlCommand(DropCommand, connection);
                command.ExecuteNonQuery();

                command.CommandText = CreateCommand;
                command.ExecuteNonQuery();
            }
        }
    }
}