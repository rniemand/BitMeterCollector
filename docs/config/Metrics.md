# Metrics

Metrics are now handled via [RnCore.Metrics](http://www.richardn.ca/RnCore.Metrics/#/) and should work using the below sample configuration.

```json
{
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
