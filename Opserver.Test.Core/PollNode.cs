// ReSharper disable All

namespace Opserver.Test.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class PollNode
    {
        private int _isPolling;

        public string Key { get; set; }

        public string Status { get; set; }

        public abstract IEnumerable<Cache> DataPollers { get; }

        public abstract IEnumerable<Cache> CachePollers { get; }

        public Stopwatch CurrentPollDuration { get; protected set; }

        public bool IsPolling
        {
            get
            {
                return _isPolling == 1;
            }
        }

        public PollNode(string key)
        {
            Key = key;
        }

        public void TryAddToPollEngine()
        {
            PollEngine.TryAdd(this);
        }

        public async Task PollAsync()
        {
            if (Interlocked.CompareExchange(ref _isPolling, 1, 0) == 1)
            {
                // already started poll by another thread
                Current.Logger.Trace($"Node Poll [{Key}] Already Polling.");
                return;
            }

            try
            {
                Current.Logger.Trace($"Node Poll [{Key}] Started.");
                CurrentPollDuration = new Stopwatch();
                CurrentPollDuration.Start();


                // tasks are executed immediatelly when added, possibly because of async methods inside.
                // yes because of async methods.
                // try to simulate on 
                var tasks = new List<Task>();
                Parallel.ForEach(DataPollers, (poller) => tasks.Add(poller.PollGenericAsync()));

                Parallel.ForEach(CachePollers, (poller) => tasks.Add(poller.PollGenericAsync()));

                if (!tasks.Any())
                    return;

                await Task.WhenAll(tasks.ToArray());
                Current.Logger.Trace($"Node Poll [{Key}] Completed.");
            }
            catch (Exception ex)
            {
                Current.Logger.Error(ex, $"Node Poll [{Key}] Failed.");
            }
            finally
            {
                if (CurrentPollDuration != null)
                {
                    CurrentPollDuration.Stop();
                    Current.Logger.Trace($"Node Poll [{Key}] Taken: {CurrentPollDuration.Elapsed}");
                    CurrentPollDuration = null;
                }

                Interlocked.Exchange(ref _isPolling, 0);
            }
        }
    }
}