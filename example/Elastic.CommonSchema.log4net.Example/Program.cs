using log4net;
using log4net.Config;
using log4net.Core;

namespace Elastic.CommonSchema.log4net.Example;

internal class Program
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

    static void Main(string[] args)
    {
        XmlConfigurator.Configure();

        GlobalContext.Properties["GlobalProperty"] = "Example";

        Logger.Info("Welcome to example!");

        using (ThreadContext.Stacks["ThreadContextProperty"].Push("ThreadContextValue"))
        using (LogicalThreadContext.Stacks["LogicalThreadContextProperty"].Push("LogicalThreadContextValue"))
        {
            Logger.Debug("Message with context");
        }

        Logger.Warn("Something happened, but I handled it");

        Logger.Error("You better pay attention to it", new Exception("Can't touch this"));

        LogCustomEvent();
    }

    private static void LogCustomEvent()
    {
        var logger = Logger.Logger;

        var loggingEvent = new LoggingEvent(typeof(Program), logger.Repository, logger.Name, Level.Info, "Custom log message with properties", null);
        loggingEvent.Properties["CustomProperty"] = "custom-value";

        logger.Log(loggingEvent);
    }
}