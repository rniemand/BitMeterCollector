# BitMeterConfig
Configuration is defined in your applications `appsettings.json` file like so:

```json
{
  "servers": [],
  "collectionIntervalSec": 10,
  "httpServiceTimeoutMs": 750,
  "maxMissedPolls": 5,
  "backOffPeriodSeconds": 600
}
```

Information on each property is listed below:

| Property | Type | Required | Default | Notes |
| --- | --- | --- | --- | --- |
| servers | [BitMeterEndPointConfig](/docs/config/BitMeterEndPointConfig.md)[] | required | `[]` | Collection of server instances to collect stats from. |
| collectionIntervalSec | `int` | optional | `10` | Collection interval for fetching stats. |
| httpServiceTimeoutMs | `int` | optional | `750` | Amount of time to wait before cancelling a collection request. |
| maxMissedPolls | `int` | optional | `5` | Max amount of missed polls before a server will be automatically disabled. |
| backOffPeriodSeconds | `int` | optional | `600` | Amount of time to back off after a failed collection attempt. |