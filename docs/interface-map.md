# Interface Map

This document explains how the four demo parts connect and where the interfaces are implemented.

## Components

```text
Apple Watch -> iPhone App -> SQLite API -> SQLite Database
                         ^          ^
                         |          |
                         +----------+-> Unity / Quest 3 VR
```

## Apple Watch To iPhone

Purpose: collect live heart rate and send samples to the paired iPhone.

Implementation files:

```text
sensor/apple-watch/WatchHeartRateManager.swift
sensor/apple-watch/WatchHeartRateView.swift
sensor/apple-watch/WatchHeartRateApp.swift
sensor/apple-watch/iPhoneWatchConnectivityBridge.swift
```

Interface:

- Apple Watch uses HealthKit workout mode to receive live heart-rate samples.
- WatchConnectivity sends samples from Watch to iPhone.
- The iPhone bridge receives those samples and posts them to the local API when the demo is running.

## iPhone To SQLite API

Purpose: bridge Watch samples to the backend and read dashboard/database data.

Implementation files:

```text
sensor/apple-watch/iPhoneHeartRateBridgeView.swift
sensor/apple-watch/iPhoneWatchConnectivityBridge.swift
sensor/apple-watch/HeartWatchAPIClient.swift
sensor/apple-watch/HeartWatchModels.swift
```

Main API calls:

```text
POST /api/samples       writes Apple Watch heart-rate samples
GET  /api/demo/status   reads whether the VR demo is running
GET  /api/latest        reads the latest sample
GET  /api/samples       reads chart history
GET  /api/db/summary    reads counts for the iPhone overview
GET  /api/db/tables     reads SQLite table schema and recent rows
GET  /api/events        reads avatar and VR events
GET  /api/chat          reads dialogue rows
POST /api/chat/records  test/write dialogue rows from the iPhone VR tab
```

## SQLite API To Database

Purpose: store all demo records in a local SQLite database.

Implementation files:

```text
apps/api/src/server.js
apps/api/src/db.js
database/schema.sql
```

Tables:

```text
heart_rate_samples  Apple Watch/iPhone heart-rate samples
avatar_messages     generated heart-rate-zone messages
vr_events           demo_start, demo_stop, and VR events
chat_messages       scripted user/avatar/system dialogue records
```

The API writes request logs to `logs/api-output-*.log` when started through the launcher.

## Unity / Quest 3 To SQLite API

Purpose: show the avatar scene, display current heart rate, and write VR interaction records.

Implementation files:

```text
unity/VRAvatarHeartWatch/Assets/Scripts/HeartRateReceiver.cs
unity/VRAvatarHeartWatch/Assets/Scripts/HeartRateWorldPanel.cs
unity/VRAvatarHeartWatch/Assets/Scripts/ScriptedConversationController.cs
unity/VRAvatarHeartWatch/Assets/Scripts/QuestScriptedInputController.cs
unity/VRAvatarHeartWatch/Assets/Scripts/AvatarDialoguePanel.cs
unity/VRAvatarHeartWatch/Assets/Scripts/AvatarMotionController.cs
unity/VRAvatarHeartWatch/Assets/Scenes/Quest3LivingRoomAvatar.unity
```

Unity API calls:

```text
GET  /api/latest        updates the heart-rate panel
POST /api/demo/start    Quest right-hand B starts the demo
POST /api/demo/stop     Quest right-hand Menu exits and stops live writes
POST /api/chat/records  scripted dialogue rows
```

Controls:

```text
Quest right-hand B      start demo
Quest right-hand A      next scripted line
Quest left-hand X       start/replay current script
Quest left-hand Y       switch script
Quest right-hand Menu   exit demo and stop live writes
Keyboard B              start demo in Editor
Keyboard N/Enter        next line in Editor
Keyboard X/Y            script controls in Editor
Keyboard Q              exit in Editor
```

## Demonstration Script

1. Start the Windows or macOS launcher.
2. Show `logs/demo-run-*.log`.
3. Copy the printed LAN API URL into the iPhone app Settings tab.
4. Show `GET /api/health` in iPhone Safari.
5. Start Apple Watch capture.
6. Press Quest right-hand B in VR.
7. Show iPhone Overview updating.
8. Show iPhone Data tab reading `GET /api/db/tables`.
9. Press A/X/Y in VR to create dialogue records.
10. Show `logs/api-output-*.log` with `POST /api/samples`, `GET /api/latest`, and `POST /api/chat/records`.
11. Press Quest right-hand Menu to write `demo_stop` and exit.

