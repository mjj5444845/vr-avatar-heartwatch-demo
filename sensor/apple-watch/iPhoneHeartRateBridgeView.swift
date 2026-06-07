import Charts
import SwiftUI

struct iPhoneHeartRateBridgeView: View {
    @EnvironmentObject private var healthReader: iPhoneHealthKitHeartRateReader
    @State private var apiURLText = UserDefaults.standard.string(forKey: "HeartRateApiBaseURL")
        ?? UserDefaults.standard.string(forKey: "HeartRateApiURL")?.replacingOccurrences(of: "/api/samples", with: "")
        ?? "http://127.0.0.1:8787"
    @State private var dashboard = HeartWatchDashboardData()
    @State private var syncStatus = "Waiting to sync"
    @State private var isSyncing = false

    var body: some View {
        NavigationStack {
            List {
                configurationSection
                liveHeartRateSection
                chartSection
                recordsSection
                eventsSection
                conversationSection
            }
            .navigationTitle("HeartWatch")
            .toolbar {
                Button("Sync") {
                    Task {
                        await syncDashboard()
                    }
                }
            }
            .task {
                await syncDashboard()
            }
            .refreshable {
                await syncDashboard()
            }
        }
    }

    private var configurationSection: some View {
        Section("SQLite API") {
            TextField("API base URL", text: $apiURLText)
                .textInputAutocapitalization(.never)
                .autocorrectionDisabled()
                .keyboardType(.URL)

            Button("Save API URL") {
                if let url = URL(string: apiURLText.trimmingCharacters(in: .whitespacesAndNewlines)) {
                    healthReader.apiBaseURL = url
                    healthReader.lastStatus = "API base URL saved"
                    Task {
                        await syncDashboard()
                    }
                }
            }

            LabeledContent("Health reader", value: healthReader.lastStatus)
            LabeledContent("Dashboard", value: isSyncing ? "Syncing" : syncStatus)
        }
    }

    private var liveHeartRateSection: some View {
        Section("Live Heart Rate") {
            HStack(alignment: .firstTextBaseline) {
                Text("\(dashboard.latest?.heartRate ?? healthReader.lastPostedHeartRate ?? 0)")
                    .font(.system(size: 52, weight: .bold, design: .rounded))
                Text("bpm")
                    .font(.headline)
                    .foregroundStyle(.secondary)
            }

            LabeledContent("Zone", value: dashboard.latest?.zone?.name ?? "--")
            LabeledContent("Source", value: dashboard.latest?.source ?? "iPhone Health")
            LabeledContent("Last posted", value: healthReader.lastPostedHeartRate.map { "\($0) bpm" } ?? "--")
            LabeledContent("Health sample", value: healthReader.lastSampleDate.map { $0.formatted(date: .abbreviated, time: .standard) } ?? "--")

            Button("Allow Health Access") {
                Task {
                    await healthReader.requestAuthorization()
                }
            }

            Button("Read Latest Health Sample") {
                Task {
                    await healthReader.fetchLatestAndPost()
                    await syncDashboard()
                }
            }
        }
    }

    private var chartSection: some View {
        Section("Heart-rate Chart") {
            if dashboard.samples.isEmpty {
                VStack(alignment: .leading, spacing: 8) {
                    Image(systemName: "heart.text.square")
                        .font(.title2)
                        .foregroundStyle(.secondary)
                    Text("No samples yet")
                        .font(.headline)
                    Text("Allow Health access, read the latest sample, then sync this screen.")
                        .font(.caption)
                        .foregroundStyle(.secondary)
                }
                .padding(.vertical, 12)
            } else {
                Chart(chartSamples) { sample in
                    LineMark(
                        x: .value("Sample", sample.index),
                        y: .value("Heart rate", sample.heartRate)
                    )
                    PointMark(
                        x: .value("Sample", sample.index),
                        y: .value("Heart rate", sample.heartRate)
                    )
                }
                .frame(height: 220)
                .chartYScale(domain: 45...150)
            }
        }
    }

    private var recordsSection: some View {
        Section("Heart-rate Records") {
            ForEach(dashboard.samples.prefix(12)) { sample in
                VStack(alignment: .leading, spacing: 4) {
                    Text("\(sample.heartRate) bpm")
                        .font(.headline)
                    Text("\(sample.source) · \(sample.timestamp)")
                        .font(.caption)
                        .foregroundStyle(.secondary)
                }
            }
        }
    }

    private var eventsSection: some View {
        Section("Avatar and VR Events") {
            ForEach(dashboard.events.prefix(12)) { event in
                VStack(alignment: .leading, spacing: 4) {
                    Text(event.text)
                        .font(.subheadline)
                    Text([event.type, event.zone, event.timestamp].compactMap { $0 }.joined(separator: " · "))
                        .font(.caption)
                        .foregroundStyle(.secondary)
                }
            }
        }
    }

    private var conversationSection: some View {
        Section("Conversation Records") {
            ForEach(dashboard.chatMessages.prefix(16)) { message in
                VStack(alignment: .leading, spacing: 5) {
                    Text(message.text)
                        .font(.subheadline)
                    Text("\(message.role) · \(message.messageType) · \(message.conversationInitiator)")
                        .font(.caption)
                        .foregroundStyle(.secondary)
                    if let heartRate = message.heartRate {
                        Text("\(heartRate) bpm · \(message.zone ?? "unknown")")
                            .font(.caption)
                            .foregroundStyle(.secondary)
                    }
                }
            }
        }
    }

    private var chartSamples: [ChartSample] {
        Array(dashboard.samples.prefix(32).reversed().enumerated()).map { index, sample in
            ChartSample(id: sample.id, index: index, heartRate: sample.heartRate)
        }
    }

    private func syncDashboard() async {
        guard let baseURL = URL(string: apiURLText.trimmingCharacters(in: .whitespacesAndNewlines)) else {
            syncStatus = "Invalid API URL"
            return
        }

        isSyncing = true
        defer { isSyncing = false }

        do {
            dashboard = try await HeartWatchAPIClient(baseURL: baseURL).fetchDashboardData()
            syncStatus = "Synced \(dashboard.samples.count) samples"
        } catch {
            syncStatus = "Sync failed: \(error.localizedDescription)"
        }
    }
}

private struct ChartSample: Identifiable {
    let id: String
    let index: Int
    let heartRate: Int
}
