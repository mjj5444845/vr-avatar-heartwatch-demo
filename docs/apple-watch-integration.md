# Apple Watch Direct Stream Notes

The default sensor path uses the original iPhone + Apple Watch design. A watchOS app starts a lightweight workout session, reads live heart-rate samples with HealthKit on Apple Watch, sends samples to the iPhone app with WatchConnectivity, and the iPhone app posts them to the SQLite API.

## Minimal Demo Path

1. Apple Watch app starts a workout session.
2. Apple Watch reads live heart-rate samples.
3. Apple Watch sends samples to the iPhone app with WatchConnectivity.
4. The iPhone app posts each sample to `POST /api/samples`.
5. The iPhone app fetches `/api/latest`, `/api/samples`, `/api/events`, and `/api/chat`.
6. Unity Quest 3 reads `/api/latest` for the VR heart-rate panel.

The payload contract remains stable:

```json
{
  "source": "apple_watch",
  "heartRate": 92,
  "timestamp": "2026-06-07T13:30:00.000Z"
}
```

## Use Apple Watch Directly

1. Start the local API:

```bash
npm run api:dev
```

2. Find your Mac LAN IP address.
3. Set the iPhone app API base URL to `http://YOUR_MAC_IP:8787`.
4. Install the iPhone app to the iPhone.
5. Install the Watch app to Apple Watch.
6. Open the HeartWatch iPhone app and keep it in the foreground.
7. Tap **Test API / SQLite** and confirm it passes.
8. Open HeartWatch on Apple Watch.
9. Tap **Start** and approve heart-rate access.
10. Tap **Sync** on iPhone. The chart, events, and conversations should refresh.
11. In Unity, set `HeartRateReceiver.apiBaseUrl` to `http://YOUR_MAC_IP:8787` and play or build the scene.

## Xcode Target Setup

The generated Xcode project has an iPhone target and a watchOS target:

```text
ios/HeartWatchDemo/HeartWatchDemo.xcodeproj
```

The iPhone target includes:

```text
sensor/apple-watch/iPhoneHeartRateBridgeApp.swift
sensor/apple-watch/iPhoneHeartRateBridgeView.swift
sensor/apple-watch/iPhoneWatchConnectivityBridge.swift
sensor/apple-watch/HeartWatchModels.swift
sensor/apple-watch/HeartWatchAPIClient.swift
```

The Watch target includes:

```text
sensor/apple-watch/WatchHeartRateApp.swift
sensor/apple-watch/WatchHeartRateView.swift
sensor/apple-watch/WatchHeartRateManager.swift
```

Enable these capabilities:

- Watch target: HealthKit.
- iPhone target: local network access for the Mac API.
- iPhone target: development HTTP cleartext traffic for `http://YOUR_MAC_IP:8787`.

## Confirm The Full Stream

1. Start `npm run api:dev`.
2. Set the iPhone app API base URL to `http://YOUR_MAC_IP:8787`.
3. Open the Watch app.
4. Tap **Start**.
5. Approve heart-rate access if prompted.
6. Check `http://YOUR_MAC_IP:8787/api/latest`; it should return the newest posted Health sample.
7. Open the Unity scene; the heart-rate panel reads `/api/latest`.
8. Open the iPhone app; the chart reads `/api/samples`, events read `/api/events`, and conversations read `/api/chat`.
9. Advance scripted VR dialogue; `/api/chat/records` stores message type and initiator for iPhone display.

## Privacy Notes

Heart rate is health data. Ask permission clearly, store the smallest useful data, and give the user a delete/export path.
