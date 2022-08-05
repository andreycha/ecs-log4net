using log4net.Core;
using log4net.Layout;

namespace Elastic.CommonSchema.log4net;

/// <summary>
/// Formats log event into a JSON representation that adheres to Elastic Common Schema specification
/// </summary>
public class EcsLayout : LayoutSkeleton
{
    public override string ContentType => "application/json";

    public override void ActivateOptions()
    {
        IgnoresException = false;
    }

    public override void Format(TextWriter writer, LoggingEvent loggingEvent)
    {
        var ecsEvent = loggingEvent.ToEcs();
        writer.WriteLine(ecsEvent.Serialize());
    }
}
