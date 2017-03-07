// ReSharper disable All

namespace Opserver.Test.Core
{
    using System;
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

        public Stopwatch CurrentPollDuration { get; protected set; }

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
                return;
            }

            try
            {
                Current.Logger.Trace($"Node Poll [{Key}] Started.");
                CurrentPollDuration = new Stopwatch();
                CurrentPollDuration.Start();

                var tasks = new List<Task>();
                foreach (var dataPoller in DataPollers.Where(p => !p.IsPolling && p.IsStale))
                    tasks.Add(dataPoller.PollGenericAsync());

                if (!tasks.Any())
                    return;

                Task.WaitAll(tasks.ToArray());
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