# Project Completion Notes

This document records the current working demo path: Apple Watch heart-rate samples flow into SQLite, Unity Quest 3 reads the newest sample, and the iPhone app displays stored heart-rate charts, events, and conversation records.

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
- iPhone app fetches:
  - `GET /api/samples`
  - `GET /api/events`
  - `GET /api/chat`
- Static Web page:
  - documents the project setup and test flow,
  - deploys to Vercel/GitHub Pages,
  - does not fetch live data.
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

Open the iPhone app and set the API base URL to the Mac LAN IP:

```text
http://192.168.1.20:8787
```

Tap Sync to load `/api/latest`, `/api/samples`, `/api/events`, and `/api/chat`.

## Apple Health Heart-rate Stream

The current lightweight path avoids Apple Watch app installation. Apple Watch syncs heart-rate records into the iPhone Health app, then the iPhone app reads the newest HealthKit sample:

```text
Apple Watch -> iPhone Health -> HeartWatch iPhone app -> SQLite API
```

In Xcode:

1. Open `ios/HeartWatchDemo/HeartWatchDemo.xcodeproj`.
2. Confirm the iPhone target includes:
   - `sensor/apple-watch/iPhoneHeartRateBridgeApp.swift`
   - `sensor/apple-watch/iPhoneHeartRateBridgeView.swift`
   - `sensor/apple-watch/iPhoneHealthKitHeartRateReader.swift`
   - `sensor/apple-watch/HeartWatchModels.swift`
   - `sensor/apple-watch/HeartWatchAPIClient.swift`
3. Enable HealthKit on the iPhone target.
4. Add `NSHealthShareUsageDescription` to the iPhone target Info settings.
5. Run the iPhone app on your device.
6. In the iPhone app, set the API base URL:

```text
http://YOUR_MAC_IP:8787
```

7. Tap **Allow Health Access** and approve heart-rate read access.
8. Tap **Read Latest Health Sample**.
9. Check the newest sample:

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

## Static Web Page Deployment

Vercel and GitHub Pages deploy a static React project README. They do not provide durable writable SQLite storage, and the Web page is no longer the live dashboard.

Use one of these modes:

- Static project information: deploy `apps/web` as-is.
- Local live demo: run `apps/api` on your Mac and point the iPhone app and Quest 3 to `http://YOUR_MAC_IP:8787`.
- Public live demo: host the API separately with HTTPS, or migrate storage to a hosted SQLite-compatible service.

The live chart and records are in the iPhone app, not the Web page.

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
