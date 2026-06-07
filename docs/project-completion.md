# Project Completion Notes

This document records the current working demo path: Apple Watch heart-rate samples flow into SQLite, Unity Quest 3 reads the newest sample, and the React dashboard displays stored heart-rate and conversation records.

## Current Implementation

- Unity scene: `unity/VRAvatarHeartWatch/Assets/Scenes/Quest3LivingRoomAvatar.unity`
- Avatar: Robot Kyle, facing the Quest camera on a simple daylight floor stage.
- Controls:
  - Quest left `X`: start selected dialogue.
  - Quest left `Y`: switch dialogue script.
  - Quest right `A`: next line.
  - Editor keyboard: `X`, `Y`, `N` or Enter.
- Avatar movement from Starter Assets is removed, so `WASD` and Space do not move or jump the avatar.
- Avatar gestures use the three clips in `unity/VRAvatarHeartWatch/Assets/AvatarMotion`.
- Heart-rate panel reads `GET /api/latest`.
- Scripted dialogue writes to `POST /api/chat/records`.
- Web Dashboard polls:
  - `GET /api/samples`
  - `GET /api/events`
  - `GET /api/chat`
- SQLite API stores:
  - `heart_rate_samples`
  - `avatar_messages`
  - `vr_events`
  - `chat_messages`

## What Was Fixed

The avatar gesture animations were not loading because the imported FBX motions were configured to copy Robot Kyle's Avatar rig. Unity reported a copied rig hierarchy mismatch and saved empty clip references in the scene.

The scene builder now imports the three motion FBXs as Humanoid clips with their own generated Avatar, then lets Unity Mecanim retarget those clips onto Robot Kyle. The scene now stores real clip references for:

- `saluteClip`
- `happyClip`
- `defeatedClip`

## Local End-to-End Run

Install dependencies once:

```bash
npm install
```

Start the SQLite API:

```bash
npm run api:dev
```

The default API is:

```text
http://localhost:8787
```

Find your Mac LAN IP for Quest 3 and iPhone testing. Example:

```text
http://192.168.1.20:8787
```

Start the dashboard against the API:

```bash
VITE_API_BASE_URL=http://192.168.1.20:8787 npm run web:dev
```

For same-Mac browser testing, this also works:

```bash
VITE_API_BASE_URL=http://127.0.0.1:8787 npm run web:dev
```

## Apple Watch Heart-rate Stream

Apple Watch cannot stream live heart rate directly to a normal web page. The lightweight real path is:

```text
Apple Watch HealthKit -> WatchConnectivity -> iPhone companion app -> SQLite API
```

In Xcode:

1. Create an iOS app with a watchOS companion app.
2. Add these files to the Watch target:
   - `sensor/apple-watch/WatchHeartRateApp.swift`
   - `sensor/apple-watch/WatchHeartRateView.swift`
   - `sensor/apple-watch/WatchHeartRateManager.swift`
3. Add these files to the iPhone target:
   - `sensor/apple-watch/iPhoneHeartRateBridgeApp.swift`
   - `sensor/apple-watch/iPhoneHeartRateBridgeView.swift`
   - `sensor/apple-watch/iPhoneWatchConnectivityBridge.swift`
4. Enable HealthKit on the Watch target.
5. Enable WatchConnectivity on both targets.
6. Add `NSHealthShareUsageDescription` to the Watch target Info settings.
7. Run the iPhone app and Watch app on your devices.
8. In the iPhone app, set the API URL:

```text
http://YOUR_MAC_IP:8787/api/samples
```

9. Open the Watch app, tap Start, and accept Health permissions.
10. Check the newest sample:

```text
http://YOUR_MAC_IP:8787/api/latest
```

For local HTTP testing, the iPhone app must be allowed to access the local network and cleartext HTTP. For a public demo, use HTTPS.

## Unity Quest 3 Setup

Open this project in Unity:

```text
unity/VRAvatarHeartWatch
```

Open the scene:

```text
Assets/Scenes/Quest3LivingRoomAvatar.unity
```

Set these fields in the scene to your API base URL:

- `HeartRateReceiver.apiBaseUrl`
- `ScriptedConversationController.apiBaseUrl`

For Quest 3, use your Mac LAN IP:

```text
http://YOUR_MAC_IP:8787
```

Do not use `127.0.0.1` in a Quest build, because that points to the Quest device itself.

If you need to regenerate the scene, use:

```text
VR Avatar Demo > Build Living Room Avatar Scene
```

To build to Quest 3, Unity Hub must have Android Build Support, Android SDK/NDK Tools, and OpenJDK installed for your Unity editor version.

## Web Dashboard Deployment

Vercel and GitHub Pages deploy the React dashboard. They do not provide durable writable SQLite storage.

Use one of these modes:

- Mock/static dashboard: deploy as-is.
- Local live demo: run `apps/api` on your Mac and point local devices to `http://YOUR_MAC_IP:8787`.
- Public live demo: host the API separately with HTTPS, or migrate storage to a hosted SQLite-compatible service.

Set `VITE_API_BASE_URL` to the reachable API URL when building the dashboard.

## Data Contracts

Apple Watch sample:

```json
{
  "source": "apple_watch",
  "heartRate": 92,
  "timestamp": "2026-06-07T13:30:00.000Z"
}
```

VR conversation record:

```json
{
  "role": "avatar",
  "text": "Your heart rate looks calm. Want to explore the scene at an easy pace?",
  "messageType": "sensor_prompt",
  "conversationInitiator": "avatar",
  "heartRate": 68,
  "zone": "calm"
}
```
