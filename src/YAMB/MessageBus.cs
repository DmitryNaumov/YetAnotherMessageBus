using System;
using System.Threading;
using YAMB.Dispatching;
using YAMB.Routing;
using YAMB.Transaction;

namespace YAMB
{
    internal sealed class MessageBus : IBusService
    {
        private readonly IEndpoint _endpoint;
        private readonly ITransactionFactory _transactionFactory;
        private readonly IMessageDispatcher _dispatcher;

        private readonly Thread _workerThread;
        private readonly ManualResetEvent _stopEvent;
        private readonly WaitHandle[] _waitHandles;
        private long _workerCount;

        public MessageBus(IEndpoint endpoint, ITransactionFactory transactionFactory, IMessageDispatcher dispatcher)
        {
            _endpoint = endpoint;
            _transactionFactory = transactionFactory;
            _dispatcher = dispatcher;

            _workerThread = new Thread(DoWork);
            _stopEvent = new ManualResetEvent(false);
            _waitHandles = new WaitHandle[] { _stopEvent };
        }

        public void Publish(object message)
        {
            _endpoint.Send(message);
        }

        public void PublishNow(params object[] messages)
        {
            using (var transaction = _transactionFactory.CreateTransaction())
            {
                foreach (var message in messages)
                {
                    Publish(message);
                }

                transaction.Commit();
            }
        }

        public void Start()
        {
            _workerThread.Start();
        }

        public void Stop()
        {
            _stopEvent.Set();
        }

        void IDisposable.Dispose()
        {
            Stop();
        }

        private void DoWork()
        {
            while (true)
            {
                ProcessMessages();

                // TODO: PollingPeriod
                if (WaitHandle.WaitAny(_waitHandles, 100) == 0)
                    break;
            }
        }

        private void AddWorker()
        {
            // TODO: MaxWorkerCount
            if (Interlocked.Increment(ref _workerCount) >= Environment.ProcessorCount * 2)
            {
                Interlocked.Decrement(ref _workerCount);
                return;
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    ProcessMessages();
                }
                finally
                {
                    Interlocked.Decrement(ref _workerCount);
                }
            });
        }

        private void ProcessMessages()
        {
            while (true)
            {
                try
                {
                    if (_stopEvent.WaitOne(0))
                        break;

                    using (var connection = _transactionFactory.CreateTransaction())
                    {
                        var message = _endpoint.Receive(AddWorker);
                        if (message == null)
                        {
                            break;
                        }

                        _dispatcher.Dispatch(message);

                        connection.Commit();
                    }
                }
                catch
                {
                    // TODO: add logging and handling

                    break;
                }
            }
        }
    }
}