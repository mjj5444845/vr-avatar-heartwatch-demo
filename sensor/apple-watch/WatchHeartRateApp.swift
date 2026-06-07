import SwiftUI

@main
struct WatchHeartRateApp: App {
    @StateObject private var manager = WatchHeartRateManager()

    var body: some Scene {
        WindowGroup {
            WatchHeartRateView()
                .environmentObject(manager)
                .onAppear {
                    manager.requestAuthorization()
                }
        }
    }
}
