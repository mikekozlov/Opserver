using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Opserver.Test.Core
{
    public class Cache<T> : Cache where T : class, new()
    {
        Func<Task<T>> _updateCache;

        public T Data { get; set; }

        public Task<T> DataTask { get; set; }


        public Cache()
        {
            _updateCache = async ()
             =>
            {
                Current.Logger.Trace($"Update Cache [{Key}] Started.");



                Current.Logger.Trace($"Update Cache [{Key}] Completed.");

                return await Task.FromResult(new T());
            };
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
