import Charts
import SwiftUI

struct iPhoneHeartRateBridgeView: View {
    @EnvironmentObject private var bridge: iPhoneWatchConnectivityBridge
    @State private var apiURLText = UserDefaults.standard.string(forKey: "HeartRateApiBaseURL")
        ?? UserDefaults.standard.string(forKey: "HeartRateApiURL")?.replacingOccurrences(of: "/api/samples", with: "")
        ?? "http://192.168.1.20:8787"
    @State private var dashboard = HeartWatchDashboardData()
    @State private var syncStatus = "Not synced"
    @State private var apiStatus = "Not checked"
    @State private var isSyncing = false
    @State private var selectedTableName = "heart_rate_samples"
    @State private var vrTestStatus = "Run the checks below after starting the Mac API."

    var body: some View {
        TabView {
            overviewTab
                .tabItem {
                    Label("Overview", systemImage: "heart.text.square")
                }

            dataTab
                .tabItem {
                    Label("Data", systemImage: "tablecells")
                }

            vrTestTab
                .tabItem {
                    Label("VR Test", systemImage: "visionpro")
                }

            settingsTab
                .tabItem {
                    Label("Settings", systemImage: "gearshape")
                }
        }
        .task {
            bridge.apiBaseURL = URL(string: apiURLText) ?? bridge.apiBaseURL
            await refreshAll()
        }
    }

    private var overviewTab: some View {
        NavigationStack {
            List {
                Section {
                    HStack(alignment: .firstTextBaseline) {
                        Text("\(dashboard.latest?.heartRate ?? bridge.lastPostedHeartRate ?? 0)")
                            .font(.system(size: 64, weight: .bold, design: .rounded))
                        Text("bpm")
                            .font(.title3)
                            .foregroundStyle(.secondary)
                    }

                    HStack {
                        StatusPill(title: "API", value: apiStatus)
                        StatusPill(title: "Watch", value: bridge.lastStatus)
                    }

                    if let latest = dashboard.latest {
                        LabeledContent("Zone", value: latest.zone?.name.capitalized ?? "Unknown")
                        LabeledContent("Source", value: latest.source)
                        LabeledContent("Latest sample", value: latest.timestamp)
                    } else {
                        Label("No heart-rate sample yet", systemImage: "exclamationmark.circle")
                            .foregroundStyle(.secondary)
                    }
                }

                Section("Quick Test Flow") {
                    StepRow(number: 1, title: "Mac API", detail: "Run npm run api:dev")
                    StepRow(number: 2, title: "iPhone", detail: "Keep this app open on the same Wi-Fi")
                    StepRow(number: 3, title: "Apple Watch", detail: "Open HeartWatch and tap Start")
                    StepRow(number: 4, title: "VR", detail: "Quest/Unity reads /api/latest and posts chat rows")
                }

                Section("Heart-rate Trend") {
                    if chartSamples.isEmpty {
                        EmptyState(
                            icon: "waveform.path.ecg",
                            title: "Waiting for samples",
                            message: "Start the Watch app, then tap Sync."
                        )
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
                        .frame(height: 180)
                        .chartYScale(domain: 45...150)
                    }
                }
            }
            .navigationTitle("HeartWatch")
            .toolbar {
                Button("Sync") {
                    Task {
                        await refreshAll()
                    }
                }
            }
            .refreshable {
                await refreshAll()
            }
        }
    }

    private var dataTab: some View {
        NavigationStack {
            List {
                Section("Database Summary") {
                    HStack {
                        MetricCard(title: "Samples", value: "\(dashboard.databaseSummary?.samples ?? dashboard.samples.count)")
                        MetricCard(title: "Events", value: "\(dashboard.databaseSummary?.events ?? dashboard.events.count)")
                        MetricCard(title: "Chat", value: "\(dashboard.databaseSummary?.chat ?? dashboard.chatMessages.count)")
                    }
                }

                Section("SQLite Tables") {
                    if dashboard.databaseTables.isEmpty {
                        EmptyState(
                            icon: "externaldrive.badge.questionmark",
                            title: "No schema loaded",
                            message: "Tap Sync after the API is running."
                        )
                    } else {
                        Picker("Table", selection: $selectedTableName) {
                            ForEach(dashboard.databaseTables) { table in
                                Text(table.name).tag(table.name)
                            }
                        }

                        if let table = selectedTable {
                            LabeledContent("Rows", value: "\(table.rowCount)")
                            DisclosureGroup("Columns") {
                                ForEach(table.columns) { column in
                                    VStack(alignment: .leading, spacing: 3) {
                                        Text(column.name)
                                            .font(.headline)
                                        Text(columnDescription(column))
                                            .font(.caption)
                                            .foregroundStyle(.secondary)
                                    }
                                    .padding(.vertical, 3)
                                }
                            }

                            ForEach(Array(table.rows.enumerated()), id: \.offset) { index, row in
                                DisclosureGroup("Row \(index + 1)") {
                                    ForEach(table.columns) { column in
                                        LabeledContent(column.name, value: row[column.name]?.description ?? "--")
                                    }
                                }
                            }
                        }
                    }
                }
            }
            .navigationTitle("Database")
            .toolbar {
                Button("Sync") {
                    Task {
                        await refreshAll()
                    }
                }
            }
            .refreshable {
                await refreshAll()
            }
        }
    }

    private var vrTestTab: some View {
        NavigationStack {
            List {
                Section("What VR Uses") {
                    EndpointRow(method: "GET", path: "/api/latest", detail: "Heart-rate panel and avatar mood")
                    EndpointRow(method: "POST", path: "/api/chat/records", detail: "Scripted user/avatar dialogue rows")
                    EndpointRow(method: "GET", path: "/api/chat", detail: "iPhone conversation history")
                }

                Section("Run Interface Checks") {
                    Button {
                        Task {
                            await runVRInterfaceCheck()
                        }
                    } label: {
                        Label("Post sample and dialogue test", systemImage: "play.circle.fill")
                    }
                    .buttonStyle(.borderedProminent)

                    Text(vrTestStatus)
                        .font(.footnote)
                        .foregroundStyle(.secondary)
                }

                Section("Latest VR Records") {
                    if dashboard.chatMessages.isEmpty {
                        EmptyState(icon: "bubble.left.and.bubble.right", title: "No chat rows", message: "Run the VR test or advance dialogue in Unity.")
                    } else {
                        ForEach(dashboard.chatMessages.prefix(8)) { message in
                            VStack(alignment: .leading, spacing: 4) {
                                Text(message.text)
                                    .font(.subheadline)
                                Text("\(message.role) · \(message.messageType) · \(message.conversationInitiator)")
                                    .font(.caption)
                                    .foregroundStyle(.secondary)
                            }
                        }
                    }
                }
            }
            .navigationTitle("VR Test")
            .toolbar {
                Button("Sync") {
                    Task {
                        await refreshAll()
                    }
                }
            }
        }
    }

    private var settingsTab: some View {
        NavigationStack {
            List {
                Section("Connection") {
                    TextField("http://YOUR_MAC_IP:8787", text: $apiURLText)
                        .textInputAutocapitalization(.never)
                        .autocorrectionDisabled()
                        .keyboardType(.URL)

                    Button("Save and Test") {
                        Task {
                            saveAPIURL()
                            await refreshAll()
                        }
                    }
                    .buttonStyle(.borderedProminent)

                    Button("Use Example LAN URL") {
                        apiURLText = "http://192.168.1.20:8787"
                        saveAPIURL()
                    }
                    .buttonStyle(.bordered)

                    LabeledContent("API", value: apiStatus)
                    LabeledContent("Watch bridge", value: bridge.lastStatus)
                    LabeledContent("Dashboard", value: isSyncing ? "Syncing" : syncStatus)
                }

                Section("Local Setup") {
                    StepRow(number: 1, title: "Find Mac IP", detail: "Use the Mac Wi-Fi address, not 127.0.0.1")
                    StepRow(number: 2, title: "Start API", detail: "npm run api:dev")
                    StepRow(number: 3, title: "Set URL", detail: "http://YOUR_MAC_IP:8787")
                    StepRow(number: 4, title: "Start Watch", detail: "Open HeartWatch on Apple Watch")
                }
            }
            .navigationTitle("Settings")
        }
    }

    private var selectedTable: DatabaseTableRecord? {
        dashboard.databaseTables.first { $0.name == selectedTableName } ?? dashboard.databaseTables.first
    }

    private var chartSamples: [ChartSample] {
        Array(dashboard.samples.prefix(32).reversed().enumerated()).map { index, sample in
            ChartSample(id: sample.id, index: index, heartRate: sample.heartRate)
        }
    }

    private func refreshAll() async {
        saveAPIURL()
        await checkAPI()
        await syncDashboard()
    }

    private func saveAPIURL() {
        let trimmed = apiURLText.trimmingCharacters(in: .whitespacesAndNewlines)
        guard let url = URL(string: trimmed) else {
            apiStatus = "Invalid API URL"
            return
        }
        bridge.apiBaseURL = url
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
            if !dashboard.databaseTables.isEmpty,
               !dashboard.databaseTables.contains(where: { $0.name == selectedTableName }) {
                selectedTableName = dashboard.databaseTables[0].name
            }
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
            apiStatus = summary.ok ? "Online" : "Not ok"
            dashboard.databaseSummary = summary
            if let latest = summary.latest {
                dashboard.latest = latest
            }
        } catch {
            apiStatus = "Offline"
        }
    }

    private func runVRInterfaceCheck() async {
        guard let baseURL = URL(string: apiURLText.trimmingCharacters(in: .whitespacesAndNewlines)) else {
            vrTestStatus = "Invalid API URL"
            return
        }

        do {
            let client = HeartWatchAPIClient(baseURL: baseURL)
            let latest = try await client.postTestHeartRate()
            let chat = try await client.postTestVRConversation()
            vrTestStatus = "Passed: /api/latest returned \(latest.heartRate) bpm; /api/chat/records stored \(chat.role)."
            await refreshAll()
        } catch {
            vrTestStatus = "Failed: \(error.localizedDescription)"
        }
    }

    private func columnDescription(_ column: DatabaseColumnRecord) -> String {
        var parts = [column.type]
        if column.primaryKey {
            parts.append("primary key")
        }
        if column.required {
            parts.append("required")
        }
        return parts.joined(separator: " · ")
    }
}

private struct StatusPill: View {
    let title: String
    let value: String

    var body: some View {
        VStack(alignment: .leading, spacing: 4) {
            Text(title)
                .font(.caption)
                .foregroundStyle(.secondary)
            Text(value)
                .font(.footnote.weight(.semibold))
                .lineLimit(2)
        }
        .frame(maxWidth: .infinity, alignment: .leading)
        .padding(10)
        .background(.thinMaterial)
        .clipShape(RoundedRectangle(cornerRadius: 8))
    }
}

private struct MetricCard: View {
    let title: String
    let value: String

    var body: some View {
        VStack(alignment: .leading, spacing: 4) {
            Text(value)
                .font(.title2.bold())
            Text(title)
                .font(.caption)
                .foregroundStyle(.secondary)
        }
        .frame(maxWidth: .infinity, alignment: .leading)
        .padding(10)
        .background(.thinMaterial)
        .clipShape(RoundedRectangle(cornerRadius: 8))
    }
}

private struct StepRow: View {
    let number: Int
    let title: String
    let detail: String

    var body: some View {
        HStack(alignment: .top, spacing: 12) {
            Text("\(number)")
                .font(.caption.bold())
                .frame(width: 24, height: 24)
                .background(Color.accentColor.opacity(0.16))
                .clipShape(Circle())
            VStack(alignment: .leading, spacing: 3) {
                Text(title)
                    .font(.headline)
                Text(detail)
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
        }
        .padding(.vertical, 4)
    }
}

private struct EndpointRow: View {
    let method: String
    let path: String
    let detail: String

    var body: some View {
        VStack(alignment: .leading, spacing: 4) {
            HStack {
                Text(method)
                    .font(.caption.bold())
                    .padding(.horizontal, 7)
                    .padding(.vertical, 3)
                    .background(Color.accentColor.opacity(0.16))
                    .clipShape(RoundedRectangle(cornerRadius: 5))
                Text(path)
                    .font(.system(.subheadline, design: .monospaced))
            }
            Text(detail)
                .font(.caption)
                .foregroundStyle(.secondary)
        }
        .padding(.vertical, 3)
    }
}

private struct EmptyState: View {
    let icon: String
    let title: String
    let message: String

    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            Image(systemName: icon)
                .font(.title2)
                .foregroundStyle(.secondary)
            Text(title)
                .font(.headline)
            Text(message)
                .font(.caption)
                .foregroundStyle(.secondary)
        }
        .padding(.vertical, 12)
    }
}

private struct ChartSample: Identifiable {
    let id: String
    let index: Int
    let heartRate: Int
}
