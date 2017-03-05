using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Opserver.Test.Core
{
    public class Cache<T> : Cache where T : class, new()
    {
        private int _hasData;

        private Func<Task<T>> _updateCache;

        public T Data { get; set; }

        public Task<T> DataTask { get; set; }


        public Cache(string key, Func<Task<T>> getData)
        {
            Key = key;

            _updateCache = async ()
             =>
            {
                Current.Logger.Trace($"Update Cache [{Key}] Started.");

                var data = await getData();

                IsStale = false;
                IsPolling = false;

                Current.Logger.Trace($"Update Cache [{Key}] Completed.");

                return data;
            };
        }

        public void Update()
        {
            try
            {
                DataTask = _updateCache();

            }
            catch (Exception ex)
            {
                Current.Logger.Error(ex, $"Update Cache [{Key}] Failed.");
            }
        }

        public Task<int> PollAsync(bool force = false)
        {
            if (force || (_hasData == 0 && Interlocked.CompareExchange(ref _hasData, 1, 0) == 0))
            {
                
            }

        }
    }

    public abstract class Cache
    {
        public string Key { get; set; }

        public bool IsStale { get; set; }

        public bool IsPolling { get; set; }

        public string PollStatus { get; set; }
    }
}
