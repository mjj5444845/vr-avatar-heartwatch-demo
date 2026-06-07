import SwiftUI

@main
struct iPhoneHeartRateBridgeApp: App {
    @StateObject private var bridge = iPhoneWatchConnectivityBridge.shared

    var body: some Scene {
        WindowGroup {
            iPhoneHeartRateBridgeView()
                .environmentObject(bridge)
                .onAppear {
                    bridge.start()
                }
        }
    }
}
