import Foundation
import HealthKit
import WatchConnectivity

final class WatchHeartRateManager: NSObject, ObservableObject, HKWorkoutSessionDelegate, HKLiveWorkoutBuilderDelegate {
    private let healthStore = HKHealthStore()
    private var session: HKWorkoutSession?
    private var builder: HKLiveWorkoutBuilder?

    @Published var latestHeartRate: Double?

    func requestAuthorization() {
        guard HKHealthStore.isHealthDataAvailable(),
              let heartRateType = HKObjectType.quantityType(forIdentifier: .heartRate) else {
            return
        }

        healthStore.requestAuthorization(toShare: [], read: [heartRateType]) { success, error in
            if let error {
                print("HealthKit authorization error: \(error)")
            }
            print("HealthKit authorization success: \(success)")
        }
    }

    func startWorkout() {
        let configuration = HKWorkoutConfiguration()
        configuration.activityType = .mindAndBody
        configuration.locationType = .unknown

        do {
            session = try HKWorkoutSession(healthStore: healthStore, configuration: configuration)
            builder = session?.associatedWorkoutBuilder()
            builder?.dataSource = HKLiveWorkoutDataSource(healthStore: healthStore, workoutConfiguration: configuration)
            session?.delegate = self
            builder?.delegate = self

            let startDate = Date()
            session?.startActivity(with: startDate)
            builder?.beginCollection(withStart: startDate) { success, error in
                if let error {
                    print("Workout collection error: \(error)")
                }
                print("Workout collection started: \(success)")
            }
        } catch {
            print("Failed to start workout: \(error)")
        }
    }

    func stopWorkout() {
        session?.end()
    }

    func workoutBuilder(_ workoutBuilder: HKLiveWorkoutBuilder, didCollectDataOf collectedTypes: Set<HKSampleType>) {
        guard let heartRateType = HKObjectType.quantityType(forIdentifier: .heartRate),
              collectedTypes.contains(heartRateType),
              let statistics = workoutBuilder.statistics(for: heartRateType) else {
            return
        }

        let unit = HKUnit.count().unitDivided(by: HKUnit.minute())
        guard let value = statistics.mostRecentQuantity()?.doubleValue(for: unit) else {
            return
        }

        DispatchQueue.main.async {
            self.latestHeartRate = value
            self.sendToPhone(heartRate: value)
        }
    }

    func workoutBuilderDidCollectEvent(_ workoutBuilder: HKLiveWorkoutBuilder) {}

    func workoutSession(_ workoutSession: HKWorkoutSession, didChangeTo toState: HKWorkoutSessionState, from fromState: HKWorkoutSessionState, date: Date) {}

    func workoutSession(_ workoutSession: HKWorkoutSession, didFailWithError error: Error) {
        print("Workout session failed: \(error)")
    }

    private func sendToPhone(heartRate: Double) {
        guard WCSession.default.isReachable else {
            return
        }

        WCSession.default.sendMessage([
            "source": "apple_watch",
            "heartRate": Int(heartRate.rounded()),
            "timestamp": ISO8601DateFormatter().string(from: Date())
        ], replyHandler: nil) { error in
            print("WatchConnectivity send error: \(error)")
        }
    }
}

