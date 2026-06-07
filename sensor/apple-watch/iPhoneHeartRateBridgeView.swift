import Charts
import SwiftUI

struct iPhoneHeartRateBridgeView: View {
    @EnvironmentObject private var healthReader: iPhoneHealthKitHeartRateReader
    @State private var apiURLText = UserDefaults.standard.string(forKey: "HeartRateApiBaseURL")
        ?? UserDefaults.standard.string(forKey: "HeartRateApiURL")?.replacingOccurrences(of: "/api/samples", with: "")
        ?? "http://192.168.1.174:8787"
    @State private var dashboard = HeartWatchDashboardData()
    @State private var syncStatus = "Waiting to sync"
    @State private var isSyncing = false
    @State private var apiStatus = "Not checked"

    var body: some View {
        NavigationStack {
            List {
                configurationSection
                liveHeartRateSection
                databaseSection
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
                healthReader.apiBaseURL = URL(string: apiURLText) ?? healthReader.apiBaseURL
                await checkAPI()
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
                        await checkAPI()
                        await syncDashboard()
                    }
                }
            }

            Button("Use Demo Mac URL") {
                apiURLText = "http://192.168.1.174:8787"
                healthReader.apiBaseURL = URL(string: apiURLText)!
                Task {
                    await checkAPI()
                    await syncDashboard()
                }
            }
            .buttonStyle(.bordered)

            Button("Test API / SQLite") {
                Task {
                    await checkAPI()
                    await syncDashboard()
                }
            }
            .buttonStyle(.borderedProminent)

            LabeledContent("API", value: apiStatus)
            LabeledContent("Health reader", value: healthReader.lastStatus)
            LabeledContent("Dashboard", value: isSyncing ? "Syncing" : syncStatus)
        }
    }

    private var liveHeartRateSection: some View {
        Section("Live Heart Rate") {
            HStack(alignment: .firstTextBaseline) {
                Text("\(dashboard.latest?.heartRate ?? healthReader.lastPostedHeartRate ?? healthReader.lastReadHeartRate ?? 0)")
                    .font(.system(size: 52, weight: .bold, design: .rounded))
                Text("bpm")
                    .font(.headline)
                    .foregroundStyle(.secondary)
            }

            LabeledContent("Zone", value: dashboard.latest?.zone?.name ?? "--")
            LabeledContent("Source", value: dashboard.latest?.source ?? "iPhone Health")
            LabeledContent("Latest Health read", value: healthReader.lastReadHeartRate.map { "\($0) bpm" } ?? "--")
            LabeledContent("Last posted", value: healthReader.lastPostedHeartRate.map { "\($0) bpm" } ?? "--")
            LabeledContent("Health sample", value: healthReader.lastSampleDate.map { $0.formatted(date: .abbreviated, time: .standard) } ?? "--")
            LabeledContent("Imported", value: "\(healthReader.importedCount)")

            Button("Allow Health Access") {
                Task {
                    await healthReader.requestAuthorization()
                }
            }

            Button("Read Latest Health Sample") {
                Task {
                    await healthReader.fetchLatestAndPost()
                    await checkAPI()
                    await syncDashboard()
                }
            }
            .buttonStyle(.borderedProminent)

            Button("Import Recent Health Samples") {
                Task {
                    await healthReader.importRecentSamples(limit: 30)
                    await checkAPI()
                    await syncDashboard()
                }
            }
            .buttonStyle(.borderedProminent)
        }
    }

    private var databaseSection: some View {
        Section("SQLite Database") {
            LabeledContent("Samples", value: "\(dashboard.databaseSummary?.samples ?? dashboard.samples.count)")
            LabeledContent("Avatar/events", value: "\(dashboard.databaseSummary?.events ?? dashboard.events.count)")
            LabeledContent("Conversation", value: "\(dashboard.databaseSummary?.chat ?? dashboard.chatMessages.count)")
            if let latest = dashboard.databaseSummary?.latest ?? dashboard.latest {
                VStack(alignment: .leading, spacing: 4) {
                    Text("Latest row")
                        .font(.caption)
                        .foregroundStyle(.secondary)
                    Text("\(latest.heartRate) bpm · \(latest.source)")
                        .font(.headline)
                    Text(latest.timestamp)
                        .font(.caption)
                        .foregroundStyle(.secondary)
                }
            } else {
                Text("No database rows yet")
                    .foregroundStyle(.secondary)
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

    private func checkAPI() async {
        guard let baseURL = URL(string: apiURLText.trimmingCharacters(in: .whitespacesAndNewlines)) else {
            apiStatus = "Invalid API URL"
            return
        }

        do {
            let summary = try await HeartWatchAPIClient(baseURL: baseURL).checkHealth()
            apiStatus = summary.ok ? "SQLite online" : "API returned not ok"
            dashboard.databaseSummary = summary
            if let latest = summary.latest {
                dashboard.latest = latest
            }
        } catch {
            apiStatus = "Offline: \(error.localizedDescription)"
        }
    }
}

private struct ChartSample: Identifiable {
    let id: String
    let index: Int
    let heartRate: Int
}
