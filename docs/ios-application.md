# iPhone Application Setup

The project now uses the iPhone app as the live application interface. The static Web page is only a public project README.

The iPhone app does two jobs:

- receives Apple Watch heart-rate samples through WatchConnectivity and posts them to SQLite API,
- reads the API data and displays heart-rate charts, events, and conversation records.

## Required Apple Devices

- Mac with Xcode.
- iPhone paired with Apple Watch.
- Apple Watch with Health permissions available.
- Free or paid Apple Developer account signed into Xcode.

## Xcode Project

The repo includes a generated Xcode project:

```text
ios/HeartWatchDemo/HeartWatchDemo.xcodeproj
```

Open this project directly in Xcode. It already contains:

- iPhone app target: `HeartWatchDemo`
- Apple Watch app target: `HeartWatchDemoWatchApp`
- shared schemes for both targets
- HealthKit entitlement for the watchOS target
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
iPhoneWatchConnectivityBridge.swift
HeartWatchModels.swift
HeartWatchAPIClient.swift
```

These files from `sensor/apple-watch` are included in the Watch app target:

```text
WatchHeartRateApp.swift
WatchHeartRateView.swift
WatchHeartRateManager.swift
```

Do not add both `iPhoneHeartRateBridgeApp.swift` and `WatchHeartRateApp.swift` to the same target. Each file contains an `@main` app entry.

The iPhone target uses Swift Charts, so use iOS 16 or newer.

## Capabilities And Info Settings

In Xcode, open **Signing & Capabilities**.

For the iPhone target:

1. Set your Team.
2. Enable **Automatically manage signing**.
3. Add **WatchConnectivity** if it is not already present.

For the Watch target:

1. Set your Team.
2. Enable **Automatically manage signing**.
3. Add **HealthKit**.
4. Add **WatchConnectivity**.
5. Add **Workout Processing** if Xcode offers it for your watchOS target.

In the Watch target Info settings, add:

```text
NSHealthShareUsageDescription = This demo reads heart rate to drive a VR avatar.
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

## Enable Developer Mode On Apple Watch

Development-signed watchOS apps also need the Apple Watch to be trusted for development. If the iPhone Watch app says:

```text
This app could not be installed at this time
```

check the watch first.

On Apple Watch:

1. Keep the watch unlocked and on your wrist.
2. Open **Settings > Privacy & Security > Developer Mode**.
3. Turn **Developer Mode** on.
4. Restart the watch when prompted.
5. After restart, confirm Developer Mode.

If Developer Mode is not visible on the watch:

1. Keep iPhone connected to the Mac.
2. Open `ios/HeartWatchDemo/HeartWatchDemo.xcodeproj` in Xcode.
3. Open **Window > Devices and Simulators**.
4. Select the iPhone.
5. Wait for the paired Apple Watch to appear under the iPhone.
6. Try running the `HeartWatchDemoWatchApp` scheme once.
7. Check the watch again for Developer Mode.

The watch must be visible to Xcode as a paired development device. If only the iPhone appears, the Watch app may install to the iPhone but fail when transferred to Apple Watch.

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
```

If launch is blocked after installation, trust the developer certificate on iPhone:

```text
Settings > General > VPN & Device Management > Developer App > Trust
```

## Run The Watch Stream

1. Enable Developer Mode on both iPhone and Apple Watch.
2. In Xcode, select the `HeartWatchDemoWatchApp` scheme if it did not install automatically.
3. Select the paired Apple Watch destination.
4. Run it on your paired Apple Watch.
5. Open the Watch app.
6. Tap **Start**.
7. Approve Health permission.
8. Wait for a bpm value to appear.

The Watch app starts a workout session because Apple Watch provides reliable live heart-rate updates during workouts.

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
2. On Apple Watch, tap Start and wait for heart rate.
3. On iPhone, check **Watch bridge** status. It should say `Posted to API`.
4. On iPhone, tap **Sync**. The chart and records should update.
5. In Unity/Quest 3, confirm the heart-rate panel updates.
6. Press Quest left `X` to start dialogue, left `Y` to switch scripts, and right `A` for next.
7. On iPhone, pull to refresh or tap **Sync**. Conversation records should show the VR dialogue rows.

## Static Web Page

The Web page in `apps/web` is now a project information page only. Deploy it to Vercel or GitHub Pages to explain the demo, installation flow, API contract, and test checklist.
