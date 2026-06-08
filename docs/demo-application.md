# Lightweight Demo Application

This project is a presentation demo for how four parts connect:

```text
Apple Watch -> iPhone App -> SQLite API/Database <- Unity Quest 3 VR
                                  ^
                                  |
                            iPhone Data Views
```

The goal is not to make a production health product. The goal is to make the connection visible:

- Apple Watch produces live heart-rate samples.
- iPhone receives the Watch stream and posts samples to the local API.
- SQLite stores the samples, avatar messages, VR events, and conversation rows.
- Unity/Quest reads the latest heart rate and writes VR dialogue/events.
- iPhone reads the same database API and shows what is stored.

## One-file Start

### macOS

Double-click:

```text
scripts/start-demo-macos.command
```

Or run:

```bash
./scripts/start-demo-macos.command
```

The file starts the SQLite API, prints the local network URL, and opens the built macOS Unity app if it exists. If the app has not been built yet, it opens the Unity project.

### Windows

Right-click and run with PowerShell:

```text
scripts/start-demo-windows.ps1
```

If PowerShell blocks the script during a local demo, run this once in the repo folder:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\start-demo-windows.ps1
```

The Windows script starts the same SQLite API and opens `release\Windows\VRAvatarHeartWatch.exe` if it exists.

## Build macOS, Windows, and Quest Outputs

Open the Unity project:

```text
unity/VRAvatarHeartWatch
```

Use these Unity menu items:

```text
VR Avatar Demo > Build > macOS Demo App
VR Avatar Demo > Build > Windows Demo App
VR Avatar Demo > Build > Quest 3 Android APK
```

The expected output locations are:

```text
release/macOS/VRAvatarHeartWatch.app
release/Windows/VRAvatarHeartWatch.exe
release/Quest3/VRAvatarHeartWatch.apk
```

Windows builds require the Windows Build Support module in Unity. If batchmode says `Verify that the module for StandaloneWindows64 is installed`, install that module from Unity Hub, then run the same build menu again. Quest builds require Android Build Support and OpenXR/Quest setup.

## Demo Controls

In Quest 3:

- Right-hand **B**: start the whole demo and write a `demo_start` event to SQLite.
- Right-hand **A**: advance the current scripted conversation.
- Left-hand **X**: start/replay the current script.
- Left-hand **Y**: switch to the next script.

In Unity Editor keyboard fallback:

- `B`: start the whole demo.
- `N` or `Enter`: next line.
- `X`: start/replay current script.
- `Y`: switch script.

## Demo Flow

1. Run the one-file start script on Mac or Windows.
2. Copy the printed API URL, for example `http://192.168.1.20:8787`.
3. Open the iPhone app and set the API URL in Settings.
4. Open the Apple Watch app and tap **Start**.
5. Confirm the iPhone app shows live bpm and database rows.
6. Put on Quest 3 and press right-hand **B**.
7. Watch the VR heart-rate panel update from `/api/latest`.
8. Press **A** to advance dialogue.
9. Open the iPhone app `Data` and `VR Test` tabs to show rows written by Watch and VR.

## Database As The Demo Backend

SQLite is the local backend for the demo, like a small game save/database service. It stores:

- `heart_rate_samples`: Apple Watch/iPhone heart-rate samples.
- `avatar_messages`: generated avatar messages based on heart-rate zones.
- `vr_events`: VR demo events such as `demo_start`.
- `chat_messages`: scripted user/avatar/system dialogue rows.

The API exposes the storage as readable demo data:

```text
GET  /api/demo/status
POST /api/demo/start
GET  /api/latest
POST /api/samples
GET  /api/db/summary
GET  /api/db/tables
GET  /api/db/tables/:name
POST /api/chat/records
GET  /api/chat
```

## Presentation Sentence

Use this short explanation while presenting:

> This is a four-part lightweight demo. Apple Watch streams heart rate to the iPhone app. The iPhone posts it into a local SQLite backend. Quest 3 reads the latest heart rate from that backend and writes avatar dialogue/events back into it. The iPhone app reads the same backend so we can inspect the live chart, database tables, and VR conversation history.
