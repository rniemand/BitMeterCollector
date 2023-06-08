# Docker

You can find the Docker image [here](https://hub.docker.com/repository/docker/niemandr/bitmeter-collector/general).

```text
CORE:
    IMAGE:   niemandr/bitmeter-collector
    NAME:    BitmeterCollector

PATHS
    /app/nlog.config      /mnt/user/Backups/app-data/bitmeter-collector/nlog.config
    /logs                 /mnt/user/appdata/logs/bitmeter-collector/
    /app/appsettings.json /mnt/user/Backups/app-data/bitmeter-collector/appsettings.json

IMAGE
  https://raw.githubusercontent.com/rniemand/BitMeterCollector/master/resources/images/logo.png
```

## Custom Files

### nlog.config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xsi:schemaLocation="NLog NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true" >

  <targets>
    <target xsi:type="File" name="target1" 
            fileName="/logs/BitMeterCollector.log"
            layout="${date}|${level:uppercase=true}|${message} ${exception}|${logger}|${all-event-properties}"
            />

    <target xsi:type="Console"
            name="target2"
            layout="${date}|${level:uppercase=true}|${message} ${exception}|${logger}|${all-event-properties}"
            />
  </targets>

  <rules>
    <logger name="*" minlevel="Trace" writeTo="target1,target2" />
  </rules>
</nlog>
```

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "BitMeter": {
    "servers": [
      {
        "ipAddress": "192.168.4.11",
        "name": "Name"
      }
    ]
  },
  "RnCore.Metrics": {
    "application": "BitmeterCollector",
    "enabled": true,
    "enableConsoleOutput": false,
    "environment": "dev",
    "template": "{app}/{measurement}"
  },
  "RnCore.Metrics.InfluxDb": {
    "token": "...",
    "bucket": "default",
    "org": "...",
    "url": "http://192.168.0.60:8086",
    "enabled": true
  }
}

```