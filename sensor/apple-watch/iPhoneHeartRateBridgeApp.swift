import SwiftUI

@main
struct iPhoneHeartRateBridgeApp: App {
    @StateObject private var healthReader = iPhoneHealthKitHeartRateReader.shared

    var body: some Scene {
        WindowGroup {
            iPhoneHeartRateBridgeView()
                .environmentObject(healthReader)
        }
    }
}
