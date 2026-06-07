import Foundation
import HealthKit

@MainActor
final class iPhoneHealthKitHeartRateReader: ObservableObject {
    static let shared = iPhoneHealthKitHeartRateReader()

    @Published var lastPostedHeartRate: Int?
    @Published var lastSampleDate: Date?
    @Published var lastStatus = "Health access not requested"

    private let healthStore = HKHealthStore()

    var apiBaseURL: URL {
        get {
            let stored = UserDefaults.standard.string(forKey: "HeartRateApiBaseURL")
                ?? UserDefaults.standard.string(forKey: "HeartRateApiURL")?.replacingOccurrences(of: "/api/samples", with: "")
                ?? "http://127.0.0.1:8787"
            return URL(string: stored)!
        }
        set {
            UserDefaults.standard.set(newValue.absoluteString.trimmingCharacters(in: CharacterSet(charactersIn: "/")), forKey: "HeartRateApiBaseURL")
        }
    }

    func requestAuthorization() async {
        guard HKHealthStore.isHealthDataAvailable(),
              let heartRateType = HKObjectType.quantityType(forIdentifier: .heartRate) else {
            lastStatus = "Health data unavailable on this iPhone"
            return
        }

        do {
            try await healthStore.requestAuthorization(toShare: [], read: [heartRateType])
            lastStatus = "Health access ready"
        } catch {
            lastStatus = "Health access failed: \(error.localizedDescription)"
        }
    }

    func fetchLatestAndPost() async {
        guard let sample = await fetchLatestHeartRateSample() else {
            return
        }

        await postSample(heartRate: sample.heartRate, timestamp: sample.timestamp)
    }

    private func fetchLatestHeartRateSample() async -> (heartRate: Int, timestamp: Date)? {
        guard let heartRateType = HKObjectType.quantityType(forIdentifier: .heartRate) else {
            lastStatus = "Heart rate type unavailable"
            return nil
        }

        let sort = NSSortDescriptor(key: HKSampleSortIdentifierEndDate, ascending: false)
        let predicate = HKQuery.predicateForSamples(
            withStart: Calendar.current.date(byAdding: .day, value: -7, to: Date()),
            end: Date(),
            options: .strictEndDate
        )

        return await withCheckedContinuation { continuation in
            let query = HKSampleQuery(
                sampleType: heartRateType,
                predicate: predicate,
                limit: 1,
                sortDescriptors: [sort]
            ) { [weak self] _, samples, error in
                Task { @MainActor in
                    if let error {
                        self?.lastStatus = "Health read failed: \(error.localizedDescription)"
                        continuation.resume(returning: nil)
                        return
                    }

                    guard let sample = samples?.first as? HKQuantitySample else {
                        self?.lastStatus = "No synced heart-rate samples found"
                        continuation.resume(returning: nil)
                        return
                    }

                    let unit = HKUnit.count().unitDivided(by: .minute())
                    let bpm = Int(sample.quantity.doubleValue(for: unit).rounded())
                    self?.lastSampleDate = sample.endDate
                    self?.lastStatus = "Read \(bpm) bpm from Health"
                    continuation.resume(returning: (heartRate: bpm, timestamp: sample.endDate))
                }
            }

            healthStore.execute(query)
        }
    }

    private func postSample(heartRate: Int, timestamp: Date) async {
        let sampleURL = URL(string: "/api/samples", relativeTo: apiBaseURL)!.absoluteURL
        var request = URLRequest(url: sampleURL)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")

        let payload: [String: Any] = [
            "id": "health-\(Int(timestamp.timeIntervalSince1970))-\(heartRate)",
            "source": "iphone_health",
            "heartRate": heartRate,
            "timestamp": ISO8601DateFormatter().string(from: timestamp)
        ]

        do {
            request.httpBody = try JSONSerialization.data(withJSONObject: payload)
            let (_, response) = try await URLSession.shared.data(for: request)
            let code = (response as? HTTPURLResponse)?.statusCode ?? 0
            lastPostedHeartRate = heartRate
            lastStatus = code >= 200 && code < 300 ? "Posted \(heartRate) bpm to API" : "API returned \(code)"
        } catch {
            lastStatus = "Post failed: \(error.localizedDescription)"
        }
    }
}
