import SwiftUI

struct iPhoneHeartRateBridgeView: View {
    @EnvironmentObject private var bridge: iPhoneWatchConnectivityBridge
    @State private var apiURLText = UserDefaults.standard.string(forKey: "HeartRateApiURL") ?? "http://127.0.0.1:8787/api/samples"

    var body: some View {
        NavigationStack {
            Form {
                Section("SQLite API") {
                    TextField("API URL", text: $apiURLText)
                        .textInputAutocapitalization(.never)
                        .autocorrectionDisabled()

                    Button("Save API URL") {
                        if let url = URL(string: apiURLText) {
                            bridge.apiURL = url
                            bridge.lastStatus = "API URL saved"
                        }
                    }
                }

                Section("Live status") {
                    LabeledContent("Last heart rate", value: bridge.lastPostedHeartRate.map { "\($0) bpm" } ?? "--")
                    Text(bridge.lastStatus)
                }
            }
            .navigationTitle("Heart Bridge")
        }
    }
}
