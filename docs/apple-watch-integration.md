# Apple Watch Integration Notes

The real-time path requires a watchOS app. Apple Watch does not expose a live browser heart-rate stream directly, so the smallest reliable implementation is HealthKit on watchOS plus WatchConnectivity to an iPhone companion app.

## Minimal Real Path

1. Create a watchOS app with HealthKit permission for heart-rate reading.
2. Start an `HKWorkoutSession` and collect live samples with `HKLiveWorkoutBuilder`.
3. Send samples from Watch to iPhone through WatchConnectivity.
4. The iPhone app posts samples to `POST /api/samples`.
5. Keep the payload contract stable:

```json
{
  "source": "apple_watch",
  "heartRate": 92,
  "timestamp": "2026-06-07T13:30:00.000Z"
}
```

## Demo Bridge Options

- **Fastest local demo**: watchOS app sends to an iPhone companion, companion posts to the local SQLite API on your Mac.
- **Hosted demo**: companion app posts to a public API that stores data in SQLite-compatible storage.
- **No native app**: import Health export data as JSON/CSV for replay mode.

## Use Your Apple Watch For Live Heart Rate

1. Start the local API:

```bash
npm run api:dev
```

2. Find your Mac LAN IP address.
3. Set the iPhone bridge API URL to `http://YOUR_MAC_IP:8787/api/samples`.
4. Build and run the iPhone + watchOS app from Xcode.
5. Open the Watch app, tap start, and approve Health permission.
6. Open the React dashboard and sync from the API.

## Privacy Notes

Heart rate is health data. Ask permission clearly, store the smallest useful data, and give the user a delete/export path.
