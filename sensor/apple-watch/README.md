# Apple Watch / iPhone Code Notes

This folder contains the Swift source for the iPhone app and Apple Watch app.

The demo flow is documented in:

```text
../../docs/demo-application.md
../../docs/ios-application.md
```

Target split:

- iPhone target: `iPhoneHeartRateBridgeApp.swift`, `iPhoneHeartRateBridgeView.swift`, `iPhoneWatchConnectivityBridge.swift`, `HeartWatchModels.swift`, `HeartWatchAPIClient.swift`
- Watch target: `WatchHeartRateApp.swift`, `WatchHeartRateView.swift`, `WatchHeartRateManager.swift`

During the demo, press right-hand **B** in Quest 3 first, then tap **Start** on Apple Watch. When the VR app exits through the Quest right-hand **Menu** button, the iPhone app stops writing live samples to the backend.
