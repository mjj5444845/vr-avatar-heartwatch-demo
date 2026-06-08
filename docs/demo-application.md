# Lightweight Demo Application

This project demonstrates how four parts connect:

```text
Apple Watch -> iPhone App -> SQLite API/Database <- Unity Quest 3 VR
                                  ^
                                  |
                            iPhone Data Views
```

It is not a production health product. It is a presentation demo:

- Apple Watch collects live heart-rate data.
- iPhone App receives the Watch stream and writes it to the local SQLite backend.
- SQLite stores heart-rate samples, avatar messages, VR events, and dialogue rows.
- Unity/Quest reads the latest heart rate and writes VR start, exit, and dialogue events back to the database.
- iPhone App reads from the same backend to show charts, table structure, and stored records.

## One-file Start

### macOS

Double-click or run:

```text
scripts/start-demo-macos.command
```

This starts the SQLite API, prints the computer LAN URL, and opens the built macOS Unity app if it exists. If the app has not been built yet, it opens the Unity project.

### Windows

Run in PowerShell:

```text
scripts/start-demo-windows.ps1
```

If PowerShell blocks the script during a local demo, run this from the repo root:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\start-demo-windows.ps1
```

The Windows script starts the same SQLite API and opens:

```text
release\Windows\VRAvatarHeartWatch.exe
```

### Copy Package For Windows

If you want to run the demo on a separate Windows machine, copy the repo after dependencies are installed, or clone the repo on Windows and run `npm install` once.

Minimum Windows demo package:

```text
apps/api
database
scripts/start-demo-windows.ps1
package.json
package-lock.json
release/Windows
```

Use the repo root as the working folder. The PowerShell script starts the local API from `apps/api`, creates/uses the SQLite database through `database/schema.sql`, and launches `release\Windows\VRAvatarHeartWatch.exe`.

The iPhone app must use the Windows computer's LAN URL, not `localhost`. Use the URL printed by the PowerShell script, for example:

```text
http://WINDOWS_IP:8787
```

## Build Outputs

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

Expected outputs:

```text
release/macOS/VRAvatarHeartWatch.app
release/Windows/VRAvatarHeartWatch.exe
release/Quest3/VRAvatarHeartWatch.apk
```

Windows builds require Unity Windows Build Support. If batch mode reports that the `StandaloneWindows64` module is missing, install that module in Unity Hub and run the same build menu again.

## VR Controls

Quest 3:

- Right-hand **B**: start the demo and write `demo_start`.
- Right-hand **A**: advance the current scripted dialogue.
- Left-hand **X**: start or replay the current script.
- Left-hand **Y**: switch to the next script.
- Right-hand **Menu**: exit the VR app, write `demo_stop`, and stop live recording.

Unity Editor keyboard:

- `B`: start the demo.
- `N` or `Enter`: next line.
- `X`: start or replay the current script.
- `Y`: switch script.
- `Q`: exit and stop recording.

## Presentation Flow

1. Run the macOS or Windows one-file launcher.
2. Copy the printed API URL, for example `http://192.168.1.20:8787`.
3. Enter that API URL in the iPhone app Settings tab.
4. Press right-hand **B** in Quest 3 to start the demo.
5. Tap **Start** in the Apple Watch app.
6. The iPhone app refreshes heart rate, database tables, and VR records every 3 seconds.
7. Press **A** in Quest 3 to advance dialogue.
8. Inspect writes in the iPhone app Data and VR tabs.
9. End the demo by pressing the Quest right-hand **Menu** button. The VR app exits, writes `demo_stop`, and iPhone stops writing live samples to the backend.
10. Tap **Stop** on Apple Watch to end the workout session.

## SQLite As A Small Backend

SQLite is the local backend for the demo, similar to a lightweight game save service. It stores:

- `heart_rate_samples`: Apple Watch/iPhone heart-rate samples.
- `avatar_messages`: avatar messages generated from heart-rate zones.
- `vr_events`: VR start, stop, and other events.
- `chat_messages`: user, avatar, and system dialogue rows.

Main endpoints:

```text
GET  /api/demo/status
POST /api/demo/start
POST /api/demo/stop
GET  /api/latest
POST /api/samples
GET  /api/db/summary
GET  /api/db/tables
GET  /api/db/tables/:name
POST /api/chat/records
GET  /api/chat
```

## Presentation Sentence

> This is a four-part connection demo. Apple Watch collects heart rate, the iPhone app writes it to a local SQLite backend, Quest 3 reads the latest heart rate in VR and writes avatar events back to the database, and the iPhone app reads the same backend so viewers can inspect the live chart, database tables, VR events, and dialogue history.
