import Foundation
import HealthKit

@MainActor
final class iPhoneHealthKitHeartRateReader: ObservableObject {
    static let shared = iPhoneHealthKitHeartRateReader()

    @Published var lastPostedHeartRate: Int?
    @Published var lastReadHeartRate: Int?
    @Published var lastSampleDate: Date?
    @Published var importedCount = 0
    @Published var isPolling = false
    @Published var pollTickCount = 0
    @Published var lastStatus = "Health access not requested"

    private let healthStore = HKHealthStore()
    private var pollingTask: Task<Void, Never>?
    private var lastPostedSampleKey: String?

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
        guard let sample = await fetchRecentHeartRateSamples(limit: 1).first else {
            return
        }

        _ = await postSample(heartRate: sample.heartRate, timestamp: sample.timestamp)
    }

    func startPolling(every seconds: UInt64 = 3, onTick: @escaping @MainActor () async -> Void = {}) {
        guard !isPolling else {
            return
        }

        isPolling = true
        lastStatus = "Live polling every \(seconds)s"
        pollingTask = Task { [weak self] in
            while !Task.isCancelled {
                await self?.pollLatestSampleOnce()
                await onTick()
                try? await Task.sleep(nanoseconds: seconds * 1_000_000_000)
            }
        }
    }

    func stopPolling() {
        pollingTask?.cancel()
        pollingTask = nil
        isPolling = false
        lastStatus = "Live polling stopped"
    }

    func importRecentSamples(limit: Int = 30) async {
        let samples = await fetchRecentHeartRateSamples(limit: limit)
        guard !samples.isEmpty else {
            return
        }

        var posted = 0
        for sample in samples.reversed() {
            if await postSample(heartRate: sample.heartRate, timestamp: sample.timestamp) {
                posted += 1
            }
        }

        importedCount = posted
        lastStatus = posted > 0 ? "Imported \(posted) Health samples" : "No samples posted"
    }

    private func pollLatestSampleOnce() async {
        pollTickCount += 1
        guard let sample = await fetchRecentHeartRateSamples(limit: 1).first else {
            return
        }

        let sampleKey = "\(Int(sample.timestamp.timeIntervalSince1970))-\(sample.heartRate)"
        guard sampleKey != lastPostedSampleKey else {
            lastStatus = "Polling: no newer Health sample"
            return
        }

        if await postSample(heartRate: sample.heartRate, timestamp: sample.timestamp) {
            lastPostedSampleKey = sampleKey
            importedCount += 1
        }
    }

    private func fetchRecentHeartRateSamples(limit: Int) async -> [(heartRate: Int, timestamp: Date)] {
        guard let heartRateType = HKObjectType.quantityType(forIdentifier: .heartRate) else {
            lastStatus = "Heart rate type unavailable"
            return []
        }

        let sort = NSSortDescriptor(key: HKSampleSortIdentifierEndDate, ascending: false)
        let predicate = HKQuery.predicateForSamples(
            withStart: Calendar.current.date(byAdding: .day, value: -30, to: Date()),
            end: Date(),
            options: .strictEndDate
        )

        return await withCheckedContinuation { continuation in
            let query = HKSampleQuery(
                sampleType: heartRateType,
                predicate: predicate,
                limit: limit,
                sortDescriptors: [sort]
            ) { [weak self] _, samples, error in
                Task { @MainActor in
                    if let error {
                        self?.lastStatus = "Health read failed: \(error.localizedDescription)"
                        continuation.resume(returning: [])
                        return
                    }

                    guard let quantitySamples = samples as? [HKQuantitySample], !quantitySamples.isEmpty else {
                        self?.lastStatus = "No synced heart-rate samples found"
                        continuation.resume(returning: [])
                        return
                    }

                    let unit = HKUnit.count().unitDivided(by: .minute())
                    let records = quantitySamples.map { sample in
                        (heartRate: Int(sample.quantity.doubleValue(for: unit).rounded()), timestamp: sample.endDate)
                    }
                    if let first = records.first {
                        self?.lastReadHeartRate = first.heartRate
                        self?.lastSampleDate = first.timestamp
                    }
                    self?.lastStatus = "Read \(records.count) Health samples"
                    continuation.resume(returning: records)
                }
            }

            healthStore.execute(query)
        }
    }

    private func postSample(heartRate: Int, timestamp: Date) async -> Bool {
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
            return code >= 200 && code < 300
        } catch {
            lastStatus = "Post failed: \(error.localizedDescription)"
            return false
        }
    }
}
