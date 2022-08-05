# Elastic.CommonSchema.log4net

log4net layout to format log events into a JSON representation that adheres to Elastic Common Schema specification.

## How to use

Specify layout type in appender's configuration:

```xml
<log4net>
    <root>
        <level value="INFO" />
        <appender-ref ref="ConsoleAppender" />
    </root>
    <appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
        <layout type="Elastic.CommonSchema.log4net.EcsLayout, Elastic.CommonSchema.log4net" />
    </appender>
</log4net>
```

See [sample application](example/Elastic.CommonSchema.log4net.Example/) for reference.

## Output

Apart from [mandatory fields](https://www.elastic.co/guide/en/ecs/current/ecs-guidelines.html#_general_guidelines), the output contains additional data:

- `log.origin.file.name` is taken from `LocationInformation`
- `log.origin.file.line` is taken from `LocationInformation`
- `log.origin.function` is taken from `LocationInformation`
- `host.hostname` is taken from `HostName` property
- `process.thread.id` is taken from `ThreadName` if it has numeric value
- `process.thread.name` is taken from `ThreadName` if it doesn't have numeric value
- `service.name` is taken from entry or calling assembly
- `service.version` is taken from entry or calling assembly
- `metadata` is taken from properties

Sample log event output (formatted for readability):

```json
{
    "@timestamp": "2022-07-31T03:50:06.3881419+02:00",
    "log.level": "INFO",
    "message": "Welcome to example!",
    "metadata": {
        "global_property": "Example"
    },
    "ecs": {
        "version": "1.5.0"
    },
    "event": {
        "timezone": "Central European Time",
        "created": "2022-08-01T14:06:28.5121651+02:00"
    },
    "host": {
        "hostname": "DESKTOP-GB7HICV"
    },
    "log": {
        "logger": "Elastic.CommonSchema.log4net.Example.Program",
        "original": null,
        "origin": {
            "file": {
                "name": "C:\\Development\\ecs-log4net\\example\\Elastic.CommonSchema.log4net.Example\\Program.cs",
                "line": 17
            },
            "function": "Main"
        }
    },
    "process": {
        "thread": {
            "id": 1
        }
    },
    "service": {
        "name": "Elastic.CommonSchema.log4net.Example",
        "version": "1.0.0.0"
    }
}
```