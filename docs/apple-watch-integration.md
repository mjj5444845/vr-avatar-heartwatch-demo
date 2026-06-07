# Apple Watch Integration Notes

The demo starts with `src/sensorMock.js`, which emits the same shape of data that a real Apple Watch bridge should send.

## Minimal Real Path

1. Create a watchOS app with HealthKit permission for heart-rate reading.
2. Subscribe to live heart-rate samples with `HKAnchoredObjectQuery` or `HKLiveWorkoutBuilder`.
3. Send samples to the web demo through a tiny bridge.
4. Keep the payload contract stable:

```json
{
  "source": "apple_watch",
  "heartRate": 92,
  "timestamp": "2026-06-07T13:30:00.000Z"
}
```

## Demo Bridge Options

- **Fastest local demo**: watchOS app sends to an iPhone companion, companion posts to a local WebSocket server.
- **Simplest cloud demo**: companion app posts to a hosted endpoint, dashboard subscribes to updates.
- **No native app**: import Health export data as JSON/CSV for replay mode.

## Privacy Notes

Heart rate is health data. Ask permission clearly, store the smallest useful data, and give the user a delete/export path.

