import Foundation

struct HeartWatchAPIClient {
    var baseURL: URL

    func fetchDashboardData() async throws -> HeartWatchDashboardData {
        async let latestResult: LatestHeartRateRecord? = optionalOrNil("/api/latest")
        async let samplesResult: [HeartRateSampleRecord] = arrayOrEmpty("/api/samples")
        async let eventsResult: [AvatarEventRecord] = arrayOrEmpty("/api/events")
        async let chatResult: [ChatMessageRecord] = arrayOrEmpty("/api/chat")
        async let summaryResult: DatabaseSummaryRecord? = optionalOrNil("/api/db/summary")
        async let tablesResult: DatabaseTablesResponse? = optionalOrNil("/api/db/tables")
        async let statusResult: DemoStatusRecord? = optionalOrNil("/api/demo/status")

        return await HeartWatchDashboardData(
            latest: latestResult,
            samples: samplesResult,
            events: eventsResult,
            chatMessages: chatResult,
            databaseSummary: summaryResult,
            databaseTables: tablesResult?.tables ?? [],
            demoStatus: statusResult
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
            "text": "Interface test: the iPhone app wrote a VR dialogue row.",
            "messageType": "system",
            "conversationInitiator": "avatar",
            "timestamp": ISO8601DateFormatter().string(from: Date())
        ]
        return try await post("/api/chat/records", payload: payload)
    }

    func fetchDemoStatus() async throws -> DemoStatusRecord {
        guard let status: DemoStatusRecord = try await fetchOptional("/api/demo/status") else {
            throw URLError(.badServerResponse)
        }
        return status
    }

    private func fetchOptional<T: Decodable>(_ path: String) async throws -> T? {
        let (data, response) = try await URLSession.shared.data(from: endpoint(path))
        try validate(response)
        if data.isEmpty || String(data: data, encoding: .utf8) == "null" {
            return nil
        }
        return try JSONDecoder().decode(T.self, from: data)
    }

    private func fetchArray<T: Decodable>(_ path: String) async throws -> [T] {
        let (data, response) = try await URLSession.shared.data(from: endpoint(path))
        try validate(response)
        return try JSONDecoder().decode([T].self, from: data)
    }

    private func optionalOrNil<T: Decodable>(_ path: String) async -> T? {
        try? await fetchOptional(path)
    }

    private func arrayOrEmpty<T: Decodable>(_ path: String) async -> [T] {
        (try? await fetchArray(path)) ?? []
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

    private func validate(_ response: URLResponse) throws {
        let statusCode = (response as? HTTPURLResponse)?.statusCode ?? 0
        guard statusCode >= 200 && statusCode < 300 else {
            throw URLError(.badServerResponse)
        }
    }
}

private struct EmptyResponse: Decodable {}
