import Foundation
import Combine
import HealthKit
import WatchConnectivity

final class WatchHeartRateManager: NSObject, ObservableObject, HKWorkoutSessionDelegate, HKLiveWorkoutBuilderDelegate {
    private let healthStore = HKHealthStore()
    private var session: HKWorkoutSession?
    private var builder: HKLiveWorkoutBuilder?
    private let isoFormatter = ISO8601DateFormatter()

    @Published var latestHeartRate: Double?
    @Published var isRunning = false
    @Published var status = "Ready"

    override init() {
        super.init()
        if WCSession.isSupported() {
            WCSession.default.delegate = self
            WCSession.default.activate()
        }
    }

    func requestAuthorization() {
        guard HKHealthStore.isHealthDataAvailable(),
              let heartRateType = HKObjectType.quantityType(forIdentifier: .heartRate) else {
            return
        }

        healthStore.requestAuthorization(toShare: [], read: [heartRateType]) { success, error in
            DispatchQueue.main.async {
                if let error {
                    self.status = "Health permission error: \(error.localizedDescription)"
                } else {
                    self.status = success ? "Health permission granted" : "Health permission denied"
                }
            }
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
                DispatchQueue.main.async {
                    if let error {
                        self.status = "Workout error: \(error.localizedDescription)"
                    } else {
                        self.isRunning = success
                        self.status = success ? "Streaming heart rate" : "Workout did not start"
                    }
                }
            }
        } catch {
            status = "Failed to start: \(error.localizedDescription)"
        }
    }

    func stopWorkout() {
        session?.end()
        builder?.endCollection(withEnd: Date()) { _, _ in }
        isRunning = false
        status = "Stopped"
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
        let payload: [String: Any] = [
            "source": "apple_watch",
            "heartRate": Int(heartRate.rounded()),
            "timestamp": isoFormatter.string(from: Date())
        ]

        if WCSession.default.isReachable {
            WCSession.default.sendMessage(payload, replyHandler: nil) { error in
                WCSession.default.transferUserInfo(payload)
                print("WatchConnectivity send error: \(error)")
            }
        } else {
            WCSession.default.transferUserInfo(payload)
        }
    }
}

extension WatchHeartRateManager: WCSessionDelegate {
    func session(_ session: WCSession, activationDidCompleteWith activationState: WCSessionActivationState, error: Error?) {
        DispatchQueue.main.async {
            if let error {
                self.status = "Watch session error: \(error.localizedDescription)"
            }
        }
    }
}
