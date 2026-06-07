# Apple Watch + iPhone Application

This folder contains Swift code for the real sensor path and the iPhone application interface.

## What You Need

- Apple Watch paired to your iPhone.
- Xcode installed on your Mac.
- Apple Developer account signing enabled in Xcode.
- Health permissions enabled by the user.

## How The Stream Works

1. The watchOS app starts a workout session.
2. HealthKit emits live heart-rate samples.
3. The watch sends each sample to the iPhone app using WatchConnectivity.
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

## Use Your Apple Watch

1. Start the SQLite API on your Mac:

```bash
npm run api:dev
```

2. Find your Mac LAN IP, for example `192.168.1.20`.
3. In the iPhone app, set the API base URL to `http://192.168.1.20:8787`.
4. Build the iPhone app from Xcode to your iPhone.
5. Build the watchOS app from Xcode to your Apple Watch if it did not install automatically.
6. Open the Watch app and tap **Start**.
7. Accept Health permissions.
8. Open the iPhone app and tap **Sync** to see chart data and records.

The watch app needs a workout session for reliable live heart-rate updates.

## Xcode File Placement

Create an iOS app with a watchOS companion app in Xcode, then add the files by target:

```text
Watch target
WatchHeartRateApp.swift
WatchHeartRateView.swift
WatchHeartRateManager.swift

iPhone target
iPhoneHeartRateBridgeApp.swift
iPhoneHeartRateBridgeView.swift
iPhoneWatchConnectivityBridge.swift
HeartWatchModels.swift
HeartWatchAPIClient.swift
```

The Watch target owns HealthKit and sends samples to the phone. The iPhone target owns the API base URL, posts to SQLite through `POST /api/samples`, and reads `/api/latest`, `/api/samples`, `/api/events`, and `/api/chat`.

Do not put both app entry files in one target, because both `WatchHeartRateApp.swift` and `iPhoneHeartRateBridgeApp.swift` contain `@main`.

## Capabilities

- Watch target: HealthKit.
- Watch target: Workout Processing if available.
- Watch and iPhone targets: WatchConnectivity.

For local HTTP testing, set the API base URL in the iPhone app to your Mac LAN IP, not `127.0.0.1`. Example:

```text
http://192.168.1.20:8787
```

Use HTTPS for a hosted or public demo.

For full iPhone installation steps, Developer Mode, signing, and test flow, see `docs/ios-application.md`.
