# Apple Health Integration Notes

The default sensor path no longer deploys an app to Apple Watch. Apple Watch records heart-rate samples normally, the iPhone Health app receives synced samples, and the HeartWatch iPhone app reads the latest heart-rate sample through HealthKit.

This is less live than a dedicated watchOS workout app, but it is much lighter and avoids Apple Watch developer-install problems.

## Minimal Demo Path

1. Apple Watch records heart-rate samples.
2. iPhone Health syncs those samples.
3. The iPhone app reads the latest HealthKit heart-rate sample.
4. The iPhone app posts the sample to `POST /api/samples`.
5. The iPhone app fetches `/api/latest`, `/api/samples`, `/api/events`, and `/api/chat`.
6. Unity Quest 3 reads `/api/latest` for the VR heart-rate panel.

The payload contract remains stable:

```json
{
  "source": "iphone_health",
  "heartRate": 92,
  "timestamp": "2026-06-07T13:30:00.000Z"
}
```

## Use Apple Watch Without A Watch App

1. Start the local API:

```bash
npm run api:dev
```

2. Find your Mac LAN IP address.
3. Set the iPhone app API base URL to `http://YOUR_MAC_IP:8787`.
4. Open the iPhone Health app.
5. Confirm data exists under **Browse > Heart > Heart Rate**.
6. Open the HeartWatch iPhone app.
7. Tap **Allow Health Access** and approve heart-rate read access.
8. Tap **Read Latest Health Sample**.
9. Tap **Sync**. The chart, events, and conversations should refresh.
10. In Unity, set `HeartRateReceiver.apiBaseUrl` to `http://YOUR_MAC_IP:8787` and play or build the scene.

## Xcode Target Setup

The generated Xcode project is iPhone-only:

```text
ios/HeartWatchDemo/HeartWatchDemo.xcodeproj
```

The iPhone target includes:

```text
sensor/apple-watch/iPhoneHeartRateBridgeApp.swift
sensor/apple-watch/iPhoneHeartRateBridgeView.swift
sensor/apple-watch/iPhoneHealthKitHeartRateReader.swift
sensor/apple-watch/HeartWatchModels.swift
sensor/apple-watch/HeartWatchAPIClient.swift
```

Enable these capabilities:

- iPhone target: HealthKit.
- iPhone target: local network access for the Mac API.
- iPhone target: development HTTP cleartext traffic for `http://YOUR_MAC_IP:8787`.

The older watchOS files are kept in `sensor/apple-watch` only as fallback reference code.

## Confirm The Full Stream

1. Start `npm run api:dev`.
2. Set the iPhone app API base URL to `http://YOUR_MAC_IP:8787`.
3. Tap **Allow Health Access**.
4. Tap **Read Latest Health Sample**.
5. Check `http://YOUR_MAC_IP:8787/api/latest`; it should return the newest posted Health sample.
6. Open the Unity scene; the heart-rate panel reads `/api/latest`.
7. Open the iPhone app; the chart reads `/api/samples`, events read `/api/events`, and conversations read `/api/chat`.
8. Advance scripted VR dialogue; `/api/chat/records` stores message type and initiator for iPhone display.

## Privacy Notes

Heart rate is health data. Ask permission clearly, store the smallest useful data, and give the user a delete/export path.
