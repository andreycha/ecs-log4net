using log4net;
using log4net.Core;
using FluentAssertions;
using log4net.Repository.Hierarchy;

namespace Elastic.CommonSchema.log4net.Tests;

public class LoggingEventConverterTests
{
    [Fact]
    public void ToEcs_AnyEvent_PopulatesBaseFields()
    {
        var loggingEvent = CreateLoggingEvent();

        var ecsEvent = loggingEvent.ToEcs();

        ecsEvent.Should().NotBeNull();
        
        ecsEvent.Timestamp.Should().Be(loggingEvent.TimeStamp);
        ecsEvent.Ecs.Version.Should().Be(Base.Version);
        ecsEvent.Message.Should().Be(loggingEvent.RenderedMessage);

    }

    private static LoggingEvent CreateLoggingEvent(Exception? exception = null)
    {
        var repositoryId = Guid.NewGuid().ToString();
        var hierarchy = (Hierarchy)LogManager.CreateRepository(repositoryId);
        hierarchy.Root.Level = Level.All;
        hierarchy.Configured = true;
        var logger = LogManager.GetLogger(repositoryId, nameof(LoggingEventConverterTests));
        return new LoggingEvent(typeof(LoggingEventConverterTests), logger.Logger.Repository, logger.Logger.Name, Level.Info, "Test message", exception);
    }

    [Fact]
    public void ToEcs_AnyEvent_PopulatesLogField()
    {
        var loggingEvent = CreateLoggingEvent();

        var ecsEvent = loggingEvent.ToEcs();

        ecsEvent.Log.Should().NotBeNull();
        
        ecsEvent.Log.Level.Should().Be(loggingEvent.Level.DisplayName);
        ecsEvent.Log.Logger.Should().Be(loggingEvent.LoggerName);
        
        ecsEvent.Log.Origin.Should().NotBeNull();
        ecsEvent.Log.Origin.Function.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToEcs_AnyEvent_PopulatesEventField()
    {
        var loggingEvent = CreateLoggingEvent();

        var ecsEvent = loggingEvent.ToEcs();

        ecsEvent.Event.Should().NotBeNull();
        ecsEvent.Event.Created.Should().Be(loggingEvent.TimeStamp);
        ecsEvent.Event.Timezone.Should().Be(TimeZoneInfo.Local.StandardName);
    }

    [Fact]
    public void ToEcs_AnyEvent_PopulatesServiceField()
    {
        var loggingEvent = CreateLoggingEvent();

        var ecsEvent = loggingEvent.ToEcs();

        ecsEvent.Service.Should().NotBeNull();
        ecsEvent.Service.Name.Should().NotBeNullOrEmpty();
        ecsEvent.Service.Version.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToEcs_AnyEvent_PopulatesProcessField()
    {
        var loggingEvent = CreateLoggingEvent();

        var ecsEvent = loggingEvent.ToEcs();

        ecsEvent.Process.Should().NotBeNull();
        ecsEvent.Process.Thread.Should().NotBeNull();
        if (int.TryParse(loggingEvent.ThreadName, out var threadId))
        {
            ecsEvent.Process.Thread.Id.Should().Be(threadId);
        }
        else
        {
            ecsEvent.Process.Thread.Name.Should().Be(loggingEvent.ThreadName);
        }
    }

    [Fact]
    public void ToEcs_AnyEvent_PopulatesHostField()
    {
        var loggingEvent = CreateLoggingEvent();

        var ecsEvent = loggingEvent.ToEcs();

        ecsEvent.Host.Should().NotBeNull();
        ecsEvent.Host.Hostname.Should().Be(Environment.MachineName);
    }

    [Fact]
    public void ToEcs_EventWithException_PopulatesErrorField()
    {
        var exception = new InvalidOperationException("Oops");
        var loggingEvent = CreateLoggingEvent(exception);

        var ecsEvent = loggingEvent.ToEcs();

        ecsEvent.Error.Should().NotBeNull();
        ecsEvent.Error.Message.Should().Be(exception.Message);
        ecsEvent.Error.Type.Should().Be(exception.GetType().FullName);
        ecsEvent.Error.StackTrace.Should().Be(exception.StackTrace);
    }

    [Fact]
    public void ToEcs_AnyEvent_PopulatesMetadataFieldWithoutLog4netProperties()
    {
        var loggingEvent = CreateLoggingEvent();

        var ecsEvent = loggingEvent.ToEcs();

        ecsEvent.Metadata.Should().BeNull();
    }

    [Fact]
    public void ToEcs_EventWithGlobalContextProperty_PopulatesMetadataField()
    {
        const string property = "global-prop";
        const string propertyValue = "global-value";
        GlobalContext.Properties[property] = propertyValue;

        try
        {
            var loggingEvent = CreateLoggingEvent();

            var ecsEvent = loggingEvent.ToEcs();

            ecsEvent.Metadata.Should().ContainKey(property);
            ecsEvent.Metadata[property].Should().Be(propertyValue);
        }
        finally
        {
            GlobalContext.Properties.Remove(property);
        }
    }

    [Fact]
    public void ToEcs_EventWithThreadContextStack_PopulatesMetadataField()
    {
        const string property = "thread-context-stack-prop";
        const string propertyValue = "thread-context-stack-value";
        using var _ = ThreadContext.Stacks[property].Push(propertyValue);

        var loggingEvent = CreateLoggingEvent();

        var ecsEvent = loggingEvent.ToEcs();

        ecsEvent.Metadata.Should().ContainKey(property);
        ecsEvent.Metadata[property].Should().Be(propertyValue);
    }

    [Fact]
    public void ToEcs_EventWithThreadContextProperty_PopulatesMetadataField()
    {
        const string property = "thread-context-prop";
        const string propertyValue = "thread-context-value";
        ThreadContext.Properties[property] = propertyValue;

        try
        {
            var loggingEvent = CreateLoggingEvent();

            var ecsEvent = loggingEvent.ToEcs();

            ecsEvent.Metadata.Should().ContainKey(property);
            ecsEvent.Metadata[property].Should().Be(propertyValue);
        }
        finally
        {
            ThreadContext.Properties.Remove(property);
        }
    }

    [Fact]
    public void ToEcs_EventInLogicalThreadContextStack_PopulatesMetadataField()
    {
        const string property = "logical-thread-context-stack-prop";
        const string propertyValue = "logical-thread-context-stack-value";
        using var _ = LogicalThreadContext.Stacks[property].Push(propertyValue);

        var loggingEvent = CreateLoggingEvent();

        var ecsEvent = loggingEvent.ToEcs();

        ecsEvent.Metadata.Should().ContainKey(property);
        ecsEvent.Metadata[property].Should().Be(propertyValue);
    }

    [Fact]
    public void ToEcs_EventWithLogicalThreadContextProperty_PopulatesMetadataField()
    {
        const string property = "logical-thread-context-prop";
        const string propertyValue = "logical-thread-context-value";
        LogicalThreadContext.Properties[property] = propertyValue;

        try
        {
            var loggingEvent = CreateLoggingEvent();

            var ecsEvent = loggingEvent.ToEcs();

            ecsEvent.Metadata.Should().ContainKey(property);
            ecsEvent.Metadata[property].Should().Be(propertyValue);
        }
        finally
        {
            LogicalThreadContext.Properties.Remove(property);
        }
    }

    [Fact]
    public void ToEcs_EventWithProperties_PopulatesMetadataField()
    {
        const string property = "additional-prop";
        const string propertyValue = "additional-value";

        var loggingEvent = CreateLoggingEvent();
        loggingEvent.Properties[property] = propertyValue;

        var ecsEvent = loggingEvent.ToEcs();

        ecsEvent.Metadata.Should().ContainKey(property);
        ecsEvent.Metadata[property].Should().Be(propertyValue);
    }
}