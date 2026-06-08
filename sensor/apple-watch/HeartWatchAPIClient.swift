import Foundation

struct HeartWatchAPIClient {
    var baseURL: URL

    func fetchDashboardData() async throws -> HeartWatchDashboardData {
        async let latest: LatestHeartRateRecord? = fetchOptional("/api/latest")
        async let samples: [HeartRateSampleRecord] = fetchArray("/api/samples")
        async let events: [AvatarEventRecord] = fetchArray("/api/events")
        async let chat: [ChatMessageRecord] = fetchArray("/api/chat")
        async let databaseSummary: DatabaseSummaryRecord? = fetchOptional("/api/db/summary")
        async let databaseTables: DatabaseTablesResponse? = fetchOptional("/api/db/tables")
        async let demoStatus: DemoStatusRecord? = fetchOptional("/api/demo/status")

        return try await HeartWatchDashboardData(
            latest: latest,
            samples: samples,
            events: events,
            chatMessages: chat,
            databaseSummary: databaseSummary,
            databaseTables: databaseTables?.tables ?? [],
            demoStatus: demoStatus
        )
    }

    func checkHealth() async throws -> DatabaseSummaryRecord {
        let summary: DatabaseSummaryRecord? = try await fetchOptional("/api/db/summary")
        guard let summary else {
            throw URLError(.badServerResponse)
        }
        return summary
    }

    func postTestHeartRate() async throws -> LatestHeartRateRecord {
        let payload: [String: Any] = [
            "source": "ios_vr_test",
            "heartRate": Int.random(in: 72...104),
            "timestamp": ISO8601DateFormatter().string(from: Date())
        ]
        let _: EmptyResponse = try await post("/api/samples", payload: payload)
        guard let latest: LatestHeartRateRecord = try await fetchOptional("/api/latest") else {
            throw URLError(.badServerResponse)
        }
        return latest
    }

    func postTestVRConversation() async throws -> ChatMessageRecord {
        let payload: [String: Any] = [
            "role": "avatar",
            "text": "VR endpoint test: avatar dialogue row written from the iPhone app.",
            "messageType": "system",
            "conversationInitiator": "avatar",
            "timestamp": ISO8601DateFormatter().string(from: Date())
        ]
        return try await post("/api/chat/records", payload: payload)
    }

    private func fetchOptional<T: Decodable>(_ path: String) async throws -> T? {
        let (data, _) = try await URLSession.shared.data(from: endpoint(path))
        if data.isEmpty || String(data: data, encoding: .utf8) == "null" {
            return nil
        }
        return try JSONDecoder().decode(T.self, from: data)
    }

    private func fetchArray<T: Decodable>(_ path: String) async throws -> [T] {
        let (data, _) = try await URLSession.shared.data(from: endpoint(path))
        return try JSONDecoder().decode([T].self, from: data)
    }

    private func post<T: Decodable>(_ path: String, payload: [String: Any]) async throws -> T {
        var request = URLRequest(url: endpoint(path))
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.httpBody = try JSONSerialization.data(withJSONObject: payload)
        let (data, response) = try await URLSession.shared.data(for: request)
        let statusCode = (response as? HTTPURLResponse)?.statusCode ?? 0
        guard statusCode >= 200 && statusCode < 300 else {
            throw URLError(.badServerResponse)
        }
        return try JSONDecoder().decode(T.self, from: data)
    }

    private func endpoint(_ path: String) -> URL {
        URL(string: path, relativeTo: baseURL)!.absoluteURL
    }
}

private struct EmptyResponse: Decodable {}
