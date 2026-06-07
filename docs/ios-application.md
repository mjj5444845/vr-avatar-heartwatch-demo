# iPhone Application Setup

The project now uses the iPhone app as the live application interface. The static Web page is only a public project README.

The iPhone app does two jobs:

- reads Apple Watch heart-rate samples that have synced into Apple Health and posts them to SQLite API,
- reads the API data and displays heart-rate charts, events, and conversation records.

## Required Apple Devices

- Mac with Xcode.
- iPhone paired with Apple Watch.
- Apple Watch already syncing heart-rate records into the iPhone Health app.
- Free or paid Apple Developer account signed into Xcode.

## Xcode Project

The repo includes a generated Xcode project:

```text
ios/HeartWatchDemo/HeartWatchDemo.xcodeproj
```

Open this project directly in Xcode. It already contains:

- iPhone app target: `HeartWatchDemo`
- HealthKit entitlement for the iPhone target
- automatic signing with Team `W9CMCJ9NYR`

The source of truth for regenerating the project is:

```text
ios/HeartWatchDemo/project.yml
```

If you need to regenerate the `.xcodeproj`, install XcodeGen and run:

```bash
cd ios/HeartWatchDemo
xcodegen generate --spec project.yml
```

If you use a different Apple Developer account later, change the Team in Xcode or update `DEVELOPMENT_TEAM` in `project.yml`.

## Swift Files In Targets

These files from `sensor/apple-watch` are included in the iPhone app target:

```text
iPhoneHeartRateBridgeApp.swift
iPhoneHeartRateBridgeView.swift
iPhoneHealthKitHeartRateReader.swift
HeartWatchModels.swift
HeartWatchAPIClient.swift
```

The older watchOS files are kept in `sensor/apple-watch` as a fallback reference, but they are no longer included in the generated Xcode project.

The iPhone target uses Swift Charts, so use iOS 16 or newer.

## Capabilities And Info Settings

In Xcode, open **Signing & Capabilities**.

For the iPhone target:

1. Set your Team.
2. Enable **Automatically manage signing**.
3. Add **HealthKit** if it is not already present.

In the iPhone target Info settings, add:

```text
NSHealthShareUsageDescription = This demo reads Apple Health heart-rate samples synced from Apple Watch.
```

For local HTTP testing from iPhone to your Mac, allow development cleartext traffic. In the iPhone target Info settings, add an App Transport Security exception for your Mac LAN IP, or use an HTTPS tunnel.

## Enable Developer Mode On iPhone

On iPhone:

1. Connect iPhone to the Mac with USB or enable wireless debugging from Xcode.
2. Open **Settings > Privacy & Security > Developer Mode**.
3. Turn **Developer Mode** on.
4. Restart the iPhone when prompted.
5. After restart, confirm Developer Mode.

If Developer Mode is not visible, try running the app from Xcode once. iOS usually reveals the setting after Xcode attempts to install a development build.

If iOS blocks the developer app certificate, open:

```text
Settings > General > VPN & Device Management
```

Then trust your Apple Developer account.

## Run The SQLite API

From the repo root:

```bash
npm install
npm run api:dev
```

Find your Mac LAN IP on the same Wi-Fi as iPhone and Quest 3. Example:

```text
http://192.168.1.20:8787
```

The iPhone and Quest 3 must use the LAN IP. `127.0.0.1` only works on the Mac itself.

## Install And Run The iPhone App

1. In Xcode, select the iPhone scheme.
2. Select your physical iPhone as the destination.
3. Press Run.
4. Keep the iPhone and Apple Watch paired so Health can sync recent heart-rate data.
5. Open the installed iPhone app.
6. Set **API base URL** to:

```text
http://YOUR_MAC_IP:8787
```

7. Tap **Save API URL**.

The command-line equivalent used during setup was:

```bash
cd ios/HeartWatchDemo
xcodebuild -project HeartWatchDemo.xcodeproj -scheme HeartWatchDemo -configuration Debug -destination 'id=YOUR_IPHONE_UDID' -derivedDataPath /tmp/HeartWatchDemoDeviceBuild -allowProvisioningUpdates build
xcrun devicectl device install app --device YOUR_COREDEVICE_ID /tmp/HeartWatchDemoDeviceBuild/Build/Products/Debug-iphoneos/HeartWatchDemo.app
```

If launch is blocked after installation, trust the developer certificate on iPhone:

```text
Settings > General > VPN & Device Management > Developer App > Trust
```

## Read Heart Rate From Apple Health

This version does not install anything on Apple Watch. The watch records heart-rate samples normally, syncs them into the iPhone Health app, and the iPhone app reads the latest synced sample.

1. Wear Apple Watch normally.
2. Open the iPhone **Health** app and confirm heart-rate data exists under **Browse > Heart > Heart Rate**.
3. Open the `HeartWatch` iPhone app.
4. Tap **Allow Health Access**.
5. Enable heart-rate read permission in the Health permission sheet.
6. Tap **Read Latest Health Sample**.
7. Tap **Sync** to refresh the chart and records from SQLite.

For a dynamic demo, tap **Start 3s Live Polling**. The iPhone app will check HealthKit every 3 seconds and post a new row to SQLite whenever the latest Health sample has a new timestamp.

This path is not as live as a watchOS workout app. It is the lightest reliable demo path because it avoids Apple Watch developer deployment entirely. If Apple Watch has not synced a newer sample into iPhone Health yet, the poller will keep showing the previous bpm and status `Polling: no newer Health sample`.

## Connect Unity Quest 3

In Unity, open:

```text
unity/VRAvatarHeartWatch/Assets/Scenes/Quest3LivingRoomAvatar.unity
```

Set these component fields to the same API base URL:

```text
HeartRateReceiver.apiBaseUrl = http://YOUR_MAC_IP:8787
ScriptedConversationController.apiBaseUrl = http://YOUR_MAC_IP:8787
```

Build and run to Quest 3 after installing Android Build Support, Android SDK/NDK Tools, and OpenJDK through Unity Hub.

## Test The Full Demo

1. On Mac, open `http://YOUR_MAC_IP:8787/api/health`.
2. On iPhone, tap **Allow Health Access**.
3. Tap **Read Latest Health Sample**. The status should say `Posted ... bpm to API`.
4. On iPhone, tap **Sync**. The chart and records should update.
5. In Unity/Quest 3, confirm the heart-rate panel updates.
6. Press Quest left `X` to start dialogue, left `Y` to switch scripts, and right `A` for next.
7. On iPhone, pull to refresh or tap **Sync**. Conversation records should show the VR dialogue rows.

## Static Web Page

The Web page in `apps/web` is now a project information page only. Deploy it to Vercel or GitHub Pages to explain the demo, installation flow, API contract, and test checklist.
