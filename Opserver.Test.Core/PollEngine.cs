// ReSharper disable All


namespace Opserver.Test.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class PollEngine
    {
        private static Thread _globalPollingThread;

        private static DateTime _startTime;

        public static int _activePolls;

        private static bool _shuttingDown;

        private static HashSet<PollNode> _pollsNodes = new HashSet<PollNode>();


        public static bool TryAdd(PollNode node)
        {
            return _pollsNodes.Add(node);
        }

        static PollEngine()
        {
            _startTime = DateTime.UtcNow;
            if (_globalPollingThread == null)
            {
                _globalPollingThread = new Thread(MonitorPollingLoop)
                {
                    Name = "GlobalPolling",
                    Priority = ThreadPriority.Lowest,
                    IsBackground = true
                };
            }
            if (!_globalPollingThread.IsAlive)
                _globalPollingThread.Start();
        }

        private static void MonitorPollingLoop()
        {
            while (!_shuttingDown)
            {
                try
                {
                    StartPollLoop();
                }
                catch (ThreadAbortException e)
                {
                    if (!_shuttingDown) Current.Logger.Error(e, "Global polling loop shutting down");
                }
                catch (Exception ex)
                {
                    Current.Logger.Error(ex, "Global polling loop shutting down");
                }
                try
                {
                    Thread.Sleep(2000);
                }
                catch (ThreadAbortException)
                {
                    // application is cycling, AND THAT'S OKAY
                }
            }
        }

        private static void StartPollLoop()
        {
            while (!_shuttingDown)
            {
                PollAndForget();
                Thread.Sleep(3000);
            }
        }

        private static void PollAndForget()
        {
            Current.Logger.Trace("Global polling started");

            foreach (var pollNode in _pollsNodes)
            {
                pollNode.PollAsync().ContinueWith(t =>
                {
                    Current.Logger.Trace($"Trace [{t.Id}] completed. Status:{t.Status}");

                    if (t.IsFaulted) Current.Logger.Error(t.Exception);
                },
                        CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously,
                        TaskScheduler.Default);
            }
        }

        public static void Stop()
        {
            _shuttingDown = false;   
        }
    }
}