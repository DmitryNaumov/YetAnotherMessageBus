using System.Data;

namespace YAMB.Transaction
{
    public interface ITransactionFactory
    {
        ITransaction CreateTransaction();

        void EnlistInTransaction(IDbCommand command);
    }
}