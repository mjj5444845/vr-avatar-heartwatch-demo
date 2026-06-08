# Apple Watch + iPhone Application

This folder contains Swift code for the sensor path and the iPhone application interface.

## What You Need

- Apple Watch paired to your iPhone.
- Xcode installed on your Mac.
- Apple Developer account signing enabled in Xcode.
- Health permissions enabled by the user on Apple Watch.

## How The Stream Works

1. The Apple Watch app starts a lightweight workout session.
2. The Apple Watch app reads live HealthKit heart-rate samples.
3. The Watch app sends samples to the iPhone app using WatchConnectivity.
4. The iPhone app posts the sample to `POST /api/samples`.
5. The iPhone app reads the SQLite API for chart data, VR events, and conversation records.
6. Unity Quest 3 reads the same SQLite API for the current heart-rate panel.

## API Payload

```json
{
  "source": "apple_watch",
  "heartRate": 92,
  "timestamp": "2026-06-07T13:30:00.000Z"
}
```

## Use Apple Watch Direct Stream

1. Start the SQLite API on your Mac:

```bash
npm run api:dev
```

2. Find your Mac LAN IP, for example `192.168.1.20`.
3. In the iPhone app, set the API base URL to `http://192.168.1.20:8787`.
4. Build the iPhone app from Xcode to your iPhone.
5. Build the Watch app from Xcode to your Apple Watch.
6. Open the HeartWatch iPhone app and keep it open.
7. Tap **Test API / SQLite**.
8. Open HeartWatch on Apple Watch.
9. Tap **Start** and approve heart-rate permission if prompted.
10. Tap **Sync** on iPhone to see chart data and records.

## Xcode File Placement

The generated Xcode project has an iPhone target and a Watch target. Add these files to the iPhone target:

```text
iPhoneHeartRateBridgeApp.swift
iPhoneHeartRateBridgeView.swift
iPhoneWatchConnectivityBridge.swift
HeartWatchModels.swift
HeartWatchAPIClient.swift
```

Add these files to the Watch target:

```text
WatchHeartRateApp.swift
WatchHeartRateView.swift
WatchHeartRateManager.swift
```

## Capabilities

- Watch target: HealthKit.

For local HTTP testing, set the API base URL in the iPhone app to your Mac LAN IP, not `127.0.0.1`. Example:

```text
http://192.168.1.20:8787
```

Use HTTPS for a hosted or public demo.

For full iPhone installation steps, Developer Mode, signing, and test flow, see `docs/ios-application.md`.
