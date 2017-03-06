// ReSharper disable All

namespace Opserver.Test.Core
{
    using System.Threading;
    using System.Threading.Tasks;

    public class PollNode
    {
        private int _isPolling ;

        public string Key { get; set; }

        public string Status { get; set; }


        public PollNode(string key)
        {
            Key = key;
        }

        public async Task PollAsync()
        {
            if (Interlocked.CompareExchange(ref _isPolling, 1, 0) == 1)
            {
                // already started poll by another thread
                return;
            }



        }
    }
}