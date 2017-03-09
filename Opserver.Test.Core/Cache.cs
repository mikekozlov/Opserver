// ReSharper disable All

namespace Opserver.Test.Core
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.Threading;
    using System.Threading.Tasks;

    public class Cache<T> : Cache where T : class
    {
        private readonly TimeSpan? duration;

        public const int _refreshInterval = 3;

        public string Description { get; set; }

        private int _hasData;

        private Func<Task<T>> _updateCache;

        private readonly SemaphoreSlim _pollSemaphoreSlim = new SemaphoreSlim(1);

        public TimeSpan? LastPollDuration { get; internal set; }

        public override bool IsStale
        {
            get { return (NextPoll ?? DateTime.MinValue) < DateTime.UtcNow; }
        }

        private T _data;

        public T Data
        {
            get
            {
                // think about locking here
                if (_data == null)
                    UpdateAsync().Wait();
                return _data;
            }
        }

        public Task<T> DataTask { get; set; }

        public Stopwatch CurrentPollDuration { get; protected set; }

        public Cache(string key, string description, Func<Task<T>> getData, TimeSpan duration)
        {
            this.duration = duration;
            Description = description;
            Key = key;

            _updateCache = async ()
             =>
            {
                _data = await getData();

                return Data;
            };
        }

        public async Task<T> UpdateAsync(bool force = false)
        {
            Current.Logger.Trace($"Update Cache [{Key}] Started. {DateTime.UtcNow.ToLongTimeString()}");

            if (!force && !IsStale)
            {
                Current.Logger.Trace($"Update Cache [{Key}] Not Stale Yet.");
                return Data;
            }

            Interlocked.Increment(ref PollEngine._activePolls);

            PollStatus = "Awaiting Semaphore";
            await _pollSemaphoreSlim.WaitAsync();
            bool errored = false;

            try
            {
                if (!force && !IsStale)
                {
                    Current.Logger.Trace($"Update Cache [{Key}] Not Stale Yet.");
                    return Data;
                }
                if (IsPolling)
                {
                    Current.Logger.Trace($"Update Cache [{Key}] Already Polling.");

                    return Data;
                }
                CurrentPollDuration = Stopwatch.StartNew();
                _isPolling = true;

                PollStatus = "Update Cache";

                await _updateCache();

                PollStatus = "Update Cache Complete";
                Interlocked.Increment(ref _pollsTotal);
                NextPoll = DateTime.UtcNow.Add(duration ?? TimeSpan.FromSeconds(_refreshInterval));
            }
            catch (Exception ex)
            {
                Current.Logger.Error(ex, $"Update Cache [{Key}] Failed.");
                errored = true;
            }
            finally
            {
                if (CurrentPollDuration != null)
                {
                    CurrentPollDuration.Stop();
                    LastPollDuration = CurrentPollDuration.Elapsed;
                    Current.Logger.Trace($"Update Cache [{Key}] Completed. Duration: {LastPollDuration}, Next Poll: {NextPoll.Value.ToLongTimeString()}");
                }

                CurrentPollDuration = null;
                _isPolling = false;
                PollStatus = errored ? "Failed" : "Completed";

                _pollSemaphoreSlim.Release();
                Interlocked.Decrement(ref PollEngine._activePolls);
            }

            return Data;
        }

        public async Task PollAsync(bool force = false)
        {
            if (IsPolling)
                return ;

            if (force || (_hasData == 0 && Interlocked.CompareExchange(ref _hasData, 1, 0) == 0))
            {
                 await UpdateAsync();
            }

            await UpdateAsync();
        }

        public async override Task PollGenericAsync(bool force = false)
        {
            await PollAsync(force);
        }
    }

    public abstract class Cache
    {
        protected bool _isPolling;

        protected int _pollsTotal;

        public string Key { get; set; }

        public abstract bool IsStale { get;}

        public DateTime? NextPoll { get; protected set; }

        public bool IsPolling => _isPolling;

        public int PollsTotal => _pollsTotal;

        public string PollStatus { get; set; }

        public int RefreshInterval { get; set; }

        public abstract Task PollGenericAsync(bool force = false);
    }
}
