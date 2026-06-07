import Foundation
import WatchConnectivity

final class iPhoneWatchConnectivityBridge: NSObject, WCSessionDelegate {
    var apiURL = URL(string: "http://127.0.0.1:8787/api/samples")!

    func start() {
        guard WCSession.isSupported() else {
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

    private func postSample(_ message: [String: Any]) async {
        var request = URLRequest(url: apiURL)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")

        do {
            request.httpBody = try JSONSerialization.data(withJSONObject: message)
            _ = try await URLSession.shared.data(for: request)
        } catch {
            print("Failed to post heart-rate sample: \(error)")
        }
    }

    func session(_ session: WCSession, activationDidCompleteWith activationState: WCSessionActivationState, error: Error?) {}
    func sessionDidBecomeInactive(_ session: WCSession) {}
    func sessionDidDeactivate(_ session: WCSession) {
        WCSession.default.activate()
    }
}

