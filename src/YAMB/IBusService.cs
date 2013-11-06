using System;

namespace YAMB
{
    public interface IBusService : IBus, IDisposable
    {
        void Start();
        void Stop();
    }
}