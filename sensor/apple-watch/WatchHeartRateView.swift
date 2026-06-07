import SwiftUI

struct WatchHeartRateView: View {
    @EnvironmentObject private var manager: WatchHeartRateManager

    var body: some View {
        VStack(spacing: 10) {
            Text(manager.latestHeartRate.map { "\(Int($0.rounded())) bpm" } ?? "-- bpm")
                .font(.system(size: 28, weight: .bold))

            Text(manager.status)
                .font(.footnote)
                .multilineTextAlignment(.center)

            Button(manager.isRunning ? "Stop" : "Start") {
                manager.isRunning ? manager.stopWorkout() : manager.startWorkout()
            }
            .buttonStyle(.borderedProminent)
        }
        .padding()
    }
}
