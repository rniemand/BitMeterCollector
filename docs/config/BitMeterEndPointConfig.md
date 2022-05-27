[Home](/README.md) / [Configuration](/docs/config/README.md) / BitMeterEndPointConfig

# BitMeterEndPointConfig
More to come...

```json
{
  "useHttps": false,
  "ipAddress": "127.0.0.1",
  "port": 9876,
  "name": "MyLaptop",
  "enabled": true
}
```

Information on each property is listed below:

| Property | Type | Required | Default | Notes |
| --- | --- | --- | --- | --- |
| useHttps | `bool` | optional | `false` | Enables the use of `https` when collecting stats. |
| ipAddress | `string` | required | - | The IP Address of the server you are collecting from. |
| port | `int` | optional | `9876` | The port that the WebUI is listening on. |
| name | `string` | required | - | The name of this server for logging and metrics. |
| enabled | `bool` | optional | `true` | Allows you to enable \ disable the current server instance. |
