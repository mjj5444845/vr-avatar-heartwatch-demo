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
    @State private var vrTestStatus = "Start the local API, then run the interface check here."
    @State private var autoRefreshEnabled = true

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
                    Label("VR", systemImage: "visionpro")
                }

            settingsTab
                .tabItem {
                    Label("Settings", systemImage: "gearshape")
                }
        }
        .task {
            bridge.apiBaseURL = URL(string: apiURLText) ?? bridge.apiBaseURL
            await refreshAll()
            while !Task.isCancelled {
                try? await Task.sleep(for: .seconds(3))
                if autoRefreshEnabled {
                    await refreshAll()
                }
            }
        }
    }

    private var overviewTab: some View {
        NavigationStack {
            List {
                Section {
                    VStack(alignment: .leading, spacing: 10) {
                        Text("Live heart rate")
                            .font(.headline)
                            .foregroundStyle(.secondary)
                        HStack(alignment: .firstTextBaseline) {
                            Text("\(dashboard.latest?.heartRate ?? bridge.lastPostedHeartRate ?? 0)")
                                .font(.system(size: 76, weight: .bold, design: .rounded))
                                .minimumScaleFactor(0.7)
                            Text("bpm")
                                .font(.title2.weight(.semibold))
                                .foregroundStyle(.secondary)
                        }
                    }
                    .padding(.vertical, 8)

                    HStack {
                        StatusPill(title: "API", value: apiStatus)
                        StatusPill(title: "Watch", value: bridge.lastStatus)
                    }

                    Toggle("Auto refresh every 3s", isOn: $autoRefreshEnabled)

                    LabeledContent("Demo state", value: demoStateText)

                    if let latest = dashboard.latest {
                        LabeledContent("Zone", value: latest.zone?.name ?? "unknown")
                        LabeledContent("Source", value: latest.source)
                        LabeledContent("Latest sample", value: latest.timestamp)
                    } else {
                        Label("No heart-rate sample yet. Press Quest right-hand B first, then start Apple Watch.", systemImage: "exclamationmark.circle")
                            .foregroundStyle(.secondary)
                    }
                }

                Section("Demo Flow") {
                    StepRow(number: 1, title: "Start computer side", detail: "Run the macOS or Windows launcher")
                    StepRow(number: 2, title: "Start VR", detail: "Press Quest right-hand B")
                    StepRow(number: 3, title: "Start Watch", detail: "Tap Start on Apple Watch")
                    StepRow(number: 4, title: "Inspect records", detail: "Heart rate, VR events, and database rows refresh here")
                }

                Section("Heart-rate Trend") {
                    if chartSamples.isEmpty {
                        EmptyState(
                            icon: "waveform.path.ecg",
                            title: "Waiting for samples",
                            message: "Press B to start the demo, then start Apple Watch."
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
            .navigationTitle("Overview")
            .navigationBarTitleDisplayMode(.inline)
            .listStyle(.plain)
            .toolbar {
                Button("Refresh") {
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
                    VStack(spacing: 12) {
                        MetricCard(title: "Samples", value: "\(dashboard.databaseSummary?.samples ?? dashboard.samples.count)")
                        MetricCard(title: "VR events", value: "\(dashboard.databaseSummary?.events ?? dashboard.events.count)")
                        MetricCard(title: "Dialogue", value: "\(dashboard.databaseSummary?.chat ?? dashboard.chatMessages.count)")
                    }
                }

                Section("SQLite Tables") {
                    if dashboard.databaseTables.isEmpty {
                        EmptyState(
                            icon: "externaldrive.badge.questionmark",
                            title: "No schema loaded",
                            message: "Start the API, then tap Refresh."
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
            .navigationBarTitleDisplayMode(.inline)
            .listStyle(.plain)
            .toolbar {
                Button("Refresh") {
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
                Section("VR API Contract") {
                    EndpointRow(method: "GET", path: "/api/latest", detail: "Read the current heart-rate panel")
                    EndpointRow(method: "POST", path: "/api/demo/start", detail: "Quest B starts the demo")
                    EndpointRow(method: "POST", path: "/api/demo/stop", detail: "VR exits and stops recording")
                    EndpointRow(method: "POST", path: "/api/chat/records", detail: "Write scripted dialogue")
                    EndpointRow(method: "GET", path: "/api/chat", detail: "Read dialogue rows")
                }

                Section("Interface Check") {
                    Button {
                        Task {
                            await runVRInterfaceCheck()
                        }
                    } label: {
                        Label("Run full interface check", systemImage: "play.circle.fill")
                            .font(.headline)
                            .frame(maxWidth: .infinity, minHeight: 44)
                    }
                    .buttonStyle(.borderedProminent)

                    Text(vrTestStatus)
                        .font(.footnote)
                        .foregroundStyle(.secondary)
                }

                Section("Latest VR Records") {
                    if let latestEvent = dashboard.demoStatus?.latestEvent {
                        VStack(alignment: .leading, spacing: 6) {
                            Text("Latest event")
                                .font(.caption.weight(.semibold))
                                .foregroundStyle(.secondary)
                            Text(latestEvent.text)
                                .font(.headline)
                            Text("\(latestEvent.type) · \(latestEvent.timestamp)")
                                .font(.caption)
                                .foregroundStyle(.secondary)
                        }
                        .padding(.vertical, 6)
                    }

                    if dashboard.chatMessages.isEmpty {
                        EmptyState(icon: "bubble.left.and.bubble.right", title: "No dialogue rows", message: "Run the interface check or advance dialogue in Unity/Quest.")
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
            .navigationTitle("VR API")
            .navigationBarTitleDisplayMode(.inline)
            .listStyle(.plain)
            .toolbar {
                Button("Refresh") {
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

                    Toggle("Auto refresh every 3s", isOn: $autoRefreshEnabled)
                    LabeledContent("API", value: apiStatus)
                    LabeledContent("Watch bridge", value: bridge.lastStatus)
                    LabeledContent("Dashboard", value: isSyncing ? "Syncing" : syncStatus)
                }

                Section("Local Startup") {
                    StepRow(number: 1, title: "Run launcher", detail: "Mac: scripts/start-demo-macos.command")
                    StepRow(number: 2, title: "Set API URL", detail: "Use http://COMPUTER_IP:8787")
                    StepRow(number: 3, title: "Start VR", detail: "Quest right-hand B")
                    StepRow(number: 4, title: "Exit VR", detail: "Quest right-hand Menu stops recording")
                }
            }
            .navigationTitle("Settings")
            .navigationBarTitleDisplayMode(.inline)
            .listStyle(.plain)
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

    private var demoStateText: String {
        if dashboard.demoStatus?.isRunning == true {
            return "Running"
        }
        return "Stopped"
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
            bridge.isPostingEnabled = dashboard.demoStatus?.isRunning == true
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
                .font(.subheadline.weight(.semibold))
                .foregroundStyle(.secondary)
            Text(value)
                .font(.body.weight(.semibold))
                .lineLimit(2)
        }
        .frame(maxWidth: .infinity, alignment: .leading)
        .padding(14)
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
                .font(.largeTitle.bold())
            Text(title)
                .font(.headline)
                .foregroundStyle(.secondary)
        }
        .frame(maxWidth: .infinity, alignment: .leading)
        .padding(16)
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
                .font(.headline.bold())
                .frame(width: 32, height: 32)
                .background(Color.accentColor.opacity(0.16))
                .clipShape(Circle())
            VStack(alignment: .leading, spacing: 3) {
                Text(title)
                    .font(.title3.weight(.semibold))
                Text(detail)
                    .font(.subheadline)
                    .foregroundStyle(.secondary)
            }
        }
        .padding(.vertical, 8)
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
                    .font(.subheadline.bold())
                    .padding(.horizontal, 9)
                    .padding(.vertical, 5)
                    .background(Color.accentColor.opacity(0.16))
                    .clipShape(RoundedRectangle(cornerRadius: 5))
                Text(path)
                    .font(.system(.body, design: .monospaced))
                    .minimumScaleFactor(0.8)
            }
            Text(detail)
                .font(.subheadline)
                .foregroundStyle(.secondary)
        }
        .padding(.vertical, 7)
    }
}

private struct EmptyState: View {
    let icon: String
    let title: String
    let message: String

    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            Image(systemName: icon)
                .font(.largeTitle)
                .foregroundStyle(.secondary)
            Text(title)
                .font(.title3.weight(.semibold))
            Text(message)
                .font(.subheadline)
                .foregroundStyle(.secondary)
        }
        .padding(.vertical, 18)
    }
}

private struct ChartSample: Identifiable {
    let id: String
    let index: Int
    let heartRate: Int
}
