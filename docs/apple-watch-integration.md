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
6. Open the React dashboard. It polls the API automatically when `VITE_API_BASE_URL` is set.
7. In Unity, set `HeartRateReceiver.apiBaseUrl` to `http://YOUR_MAC_IP:8787` and play or build the scene.

## Xcode Target Setup

Create an iOS app with a watchOS companion app, then add these files to the correct targets:

```text
Watch target
sensor/apple-watch/WatchHeartRateApp.swift
sensor/apple-watch/WatchHeartRateView.swift
sensor/apple-watch/WatchHeartRateManager.swift

iPhone target
sensor/apple-watch/iPhoneHeartRateBridgeApp.swift
sensor/apple-watch/iPhoneHeartRateBridgeView.swift
sensor/apple-watch/iPhoneWatchConnectivityBridge.swift
```

Do not compile both `@main` app files in the same target.

Enable these capabilities:

- Watch target: HealthKit.
- Watch target: Workout Processing if Xcode offers it for the selected watchOS version.
- Watch target and iPhone target: WatchConnectivity.

Add a Health usage string to the Watch target Info settings, for example:

```text
NSHealthShareUsageDescription = This demo reads heart rate to drive the VR avatar.
```

If your iPhone posts to `http://YOUR_MAC_IP:8787`, allow local networking and cleartext HTTP for development in the iPhone target. For a public demo, prefer HTTPS through a tunnel or hosted API.

## Confirm The Full Stream

1. Start `npm run api:dev`.
2. Set the iPhone app API URL to `http://YOUR_MAC_IP:8787/api/samples`.
3. Tap Start on the Watch app.
4. Check `http://YOUR_MAC_IP:8787/api/latest`; it should return the newest sample.
5. Open the Unity scene; the heart-rate panel reads `/api/latest`.
6. Open the Web Dashboard with `VITE_API_BASE_URL=http://YOUR_MAC_IP:8787`; the table and chart poll `/api/samples`.
7. Advance scripted VR dialogue; `/api/chat/records` stores message type and initiator for Web display.

## Privacy Notes

Heart rate is health data. Ask permission clearly, store the smallest useful data, and give the user a delete/export path.
