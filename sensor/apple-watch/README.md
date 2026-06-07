# Apple Health + iPhone Application

This folder contains Swift code for the sensor path and the iPhone application interface.

## What You Need

- Apple Watch paired to your iPhone and syncing heart-rate records into the Health app.
- Xcode installed on your Mac.
- Apple Developer account signing enabled in Xcode.
- Health permissions enabled by the user.

## How The Stream Works

1. Apple Watch records heart-rate samples normally.
2. The iPhone Health app receives synced samples from Apple Watch.
3. The iPhone app reads the latest HealthKit heart-rate sample.
4. The iPhone app posts the sample to `POST /api/samples`.
5. The iPhone app reads the SQLite API for chart data, VR events, and conversation records.
6. Unity Quest 3 reads the same SQLite API for the current heart-rate panel.

## API Payload

```json
{
  "source": "iphone_health",
  "heartRate": 92,
  "timestamp": "2026-06-07T13:30:00.000Z"
}
```

## Use Apple Health On iPhone

1. Start the SQLite API on your Mac:

```bash
npm run api:dev
```

2. Find your Mac LAN IP, for example `192.168.1.20`.
3. In the iPhone app, set the API base URL to `http://192.168.1.20:8787`.
4. Build the iPhone app from Xcode to your iPhone.
5. Open the Health app and confirm heart-rate records exist under **Browse > Heart > Heart Rate**.
6. Open the HeartWatch iPhone app.
7. Tap **Allow Health Access** and approve heart-rate read permission.
8. Tap **Read Latest Health Sample**.
9. Tap **Sync** to see chart data and records.

No watchOS app is required in this default demo path.

## Xcode File Placement

The generated Xcode project is iPhone-only. Add these files to the iPhone target:

```text
iPhoneHeartRateBridgeApp.swift
iPhoneHeartRateBridgeView.swift
iPhoneHealthKitHeartRateReader.swift
HeartWatchModels.swift
HeartWatchAPIClient.swift
```

The older watchOS files remain in this folder as a fallback reference, but the generated project does not use them.

## Capabilities

- iPhone target: HealthKit.

For local HTTP testing, set the API base URL in the iPhone app to your Mac LAN IP, not `127.0.0.1`. Example:

```text
http://192.168.1.20:8787
```

Use HTTPS for a hosted or public demo.

For full iPhone installation steps, Developer Mode, signing, and test flow, see `docs/ios-application.md`.
