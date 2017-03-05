using NLog;

namespace Opserver.Test.Core
{
    public static partial class Current
    {
        static Current()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }

        public static Logger Logger { get; set; }
    }
}