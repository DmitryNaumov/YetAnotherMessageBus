using System;

namespace YAMB.Transaction
{
    public interface ITransaction : IDisposable
    {
        void Commit();
    }
}