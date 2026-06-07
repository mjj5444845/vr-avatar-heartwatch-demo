# Apple Watch Heart-rate Bridge

This folder contains starter Swift code for the real sensor path.

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
5. React dashboard and Unity Quest 3 read the SQLite API.

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
3. In the iPhone companion app, set the API URL to `http://192.168.1.20:8787/api/samples`.
4. Build the watchOS app from Xcode to your Apple Watch.
5. Open the watch app and tap **Start**.
6. Accept Health permissions.
7. Open the React dashboard and click **Sync API**.

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
```

The Watch target owns HealthKit and sends samples to the phone. The iPhone target owns the API URL and posts to SQLite through `POST /api/samples`.

Do not put both app entry files in one target, because both `WatchHeartRateApp.swift` and `iPhoneHeartRateBridgeApp.swift` contain `@main`.

## Capabilities

- Watch target: HealthKit.
- Watch target: Workout Processing if available.
- Watch and iPhone targets: WatchConnectivity.

For local HTTP testing, set the API URL in the iPhone app to your Mac LAN IP, not `127.0.0.1`. Example:

```text
http://192.168.1.20:8787/api/samples
```

Use HTTPS for a hosted or public demo.
