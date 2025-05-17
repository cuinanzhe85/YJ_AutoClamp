using log4net;

namespace Common.Utils
{
    public class LogManager
    {
        public static readonly ILog IMlog = log4net.LogManager.GetLogger("Mlog");
        public static readonly ILog IExceptionLog = log4net.LogManager.GetLogger("Exception");

        public LogManager() { }
        public static void WriteMlog(string log)
        {
            IMlog.InfoFormat(log);
        }
        public static void WriteExceptionLog(string log)
        {
            IExceptionLog.ErrorFormat(log);
        }
    }
}
