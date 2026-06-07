import Foundation
import Combine
import WatchConnectivity

final class iPhoneWatchConnectivityBridge: NSObject, ObservableObject, WCSessionDelegate {
    static let shared = iPhoneWatchConnectivityBridge()

    @Published var lastPostedHeartRate: Int?
    @Published var lastStatus = "Not connected"

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

    var sampleURL: URL {
        URL(string: "/api/samples", relativeTo: apiBaseURL)!.absoluteURL
    }

    func start() {
        guard WCSession.isSupported() else {
            lastStatus = "WatchConnectivity unsupported"
            return
        }

        WCSession.default.delegate = self
        WCSession.default.activate()
    }

    func session(_ session: WCSession, didReceiveMessage message: [String : Any]) {
        Task {
            await postSample(message)
        }
    }

    func session(_ session: WCSession, didReceiveUserInfo userInfo: [String : Any] = [:]) {
        Task {
            await postSample(userInfo)
        }
    }

    private func postSample(_ message: [String: Any]) async {
        var request = URLRequest(url: sampleURL)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")

        do {
            request.httpBody = try JSONSerialization.data(withJSONObject: message)
            let (_, response) = try await URLSession.shared.data(for: request)
            let code = (response as? HTTPURLResponse)?.statusCode ?? 0
            await MainActor.run {
                self.lastPostedHeartRate = message["heartRate"] as? Int
                self.lastStatus = code >= 200 && code < 300 ? "Posted to API" : "API returned \(code)"
            }
        } catch {
            await MainActor.run {
                self.lastStatus = "Post failed: \(error.localizedDescription)"
            }
        }
    }

    func session(_ session: WCSession, activationDidCompleteWith activationState: WCSessionActivationState, error: Error?) {
        DispatchQueue.main.async {
            if let error {
                self.lastStatus = "Activation error: \(error.localizedDescription)"
            } else {
                self.lastStatus = "Watch session active"
            }
        }
    }
    func sessionDidBecomeInactive(_ session: WCSession) {}
    func sessionDidDeactivate(_ session: WCSession) {
        WCSession.default.activate()
    }
}
