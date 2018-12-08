using log4net.Appender;
using log4net.Core;

namespace LanPartyTool
{
    internal class LogEvent : AppenderSkeleton
    {
        public delegate void LogEventHandler(string message);

        public static event LogEventHandler OnLogEvent;

        protected override void Append(LoggingEvent loggingEvent)
        {
            var message = RenderLoggingEvent(loggingEvent);
            OnLogEvent?.Invoke(message);
        }
    }
}