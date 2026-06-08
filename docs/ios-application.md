# iPhone Application Setup

The project now uses the iPhone app as the live application interface. The static Web page is only a public project README.

The iPhone app does two jobs:

- receives live Apple Watch heart-rate samples over WatchConnectivity and posts them to SQLite API,
- reads the API data and displays heart-rate charts, events, and conversation records.

## Required Apple Devices

- Mac with Xcode.
- iPhone paired with Apple Watch.
- Apple Watch paired with the iPhone and available as an Xcode watchOS run destination.
- Free or paid Apple Developer account signed into Xcode.

## Xcode Project

The repo includes a generated Xcode project:

```text
ios/HeartWatchDemo/HeartWatchDemo.xcodeproj
```

Open this project directly in Xcode. It already contains:

- iPhone app target: `HeartWatchDemo`
- embedded watchOS target: `HeartWatchDemoWatchApp`
- HealthKit entitlement for the Watch target
- automatic signing. Set your own Apple Developer Team in Xcode before installing to devices.

The source of truth for regenerating the project is:

```text
ios/HeartWatchDemo/project.yml
```

If you need to regenerate the `.xcodeproj`, install XcodeGen and run:

```bash
cd ios/HeartWatchDemo
xcodegen generate --spec project.yml
```

Before a device build, change the Team in Xcode or update `DEVELOPMENT_TEAM` in `project.yml`. Do not commit a personal Team ID to a public repository.

## Swift Files In Targets

These files from `sensor/apple-watch` are included in the iPhone app target:

```text
iPhoneHeartRateBridgeApp.swift
iPhoneHeartRateBridgeView.swift
iPhoneWatchConnectivityBridge.swift
HeartWatchModels.swift
HeartWatchAPIClient.swift
```

These files are included in the Watch app target:

```text
WatchHeartRateApp.swift
WatchHeartRateView.swift
WatchHeartRateManager.swift
```

The iPhone target uses Swift Charts, so use iOS 16 or newer.

## Capabilities And Info Settings

In Xcode, open **Signing & Capabilities**.

For the iPhone target:

1. Set your Team.
2. Enable **Automatically manage signing**.
3. The iPhone target does not need HealthKit for the live direct stream.

For the Watch target:

1. Set your Team.
2. Enable **Automatically manage signing**.
3. Enable **HealthKit**.
4. Keep `WKBackgroundModes = workout-processing`.

In the Watch target Info settings, keep:

```text
NSHealthShareUsageDescription = This demo reads live heart-rate samples during a watchOS workout session.
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
4. Keep the iPhone and Apple Watch paired and nearby.
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
xcodebuild -project HeartWatchDemo.xcodeproj -scheme HeartWatchDemoWatchApp -configuration Debug -destination 'id=YOUR_WATCH_UDID' -derivedDataPath /tmp/HeartWatchDemoWatchDeviceBuild -allowProvisioningUpdates build
xcrun devicectl device install app --device YOUR_WATCH_COREDEVICE_ID /tmp/HeartWatchDemoWatchDeviceBuild/Build/Products/Debug-watchos/HeartWatch.app
```

If launch is blocked after installation, trust the developer certificate on iPhone:

```text
Settings > General > VPN & Device Management > Developer App > Trust
```

## Stream Heart Rate From Apple Watch

This version uses the original direct stream path. The Watch app starts a lightweight workout session, reads live heart-rate samples, and sends each sample to the iPhone app with WatchConnectivity.

1. Start the SQLite API on the Mac.
2. Open the `HeartWatch` iPhone app.
3. Set **API base URL** to `http://YOUR_MAC_IP:8787`.
4. Tap **Save API URL** and **Test API / SQLite**.
5. Keep the iPhone app open so the WatchConnectivity session is active.
6. Open `HeartWatch` on Apple Watch.
7. Tap **Start**.
8. Approve heart-rate access on the Watch if prompted.
9. Watch the bpm update on Apple Watch.
10. On iPhone, confirm the status changes to `Posted to API` and tap **Sync** to refresh charts and records.

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
2. On iPhone, open HeartWatch and confirm **Test API / SQLite** passes.
3. On Apple Watch, open HeartWatch, tap **Start**, and approve heart-rate access. The iPhone status should say `Posted to API`.
4. On iPhone, tap **Sync**. The chart and records should update.
5. In Unity/Quest 3, confirm the heart-rate panel updates.
6. Press Quest left `X` to start dialogue, left `Y` to switch scripts, and right `A` for next.
7. On iPhone, pull to refresh or tap **Sync**. Conversation records should show the VR dialogue rows.

## Static Web Page

The Web page in `apps/web` is now a project information page only. Deploy it to Vercel or GitHub Pages to explain the demo, installation flow, API contract, and test checklist.
