import Foundation

struct HeartWatchAPIClient {
    var baseURL: URL

    func fetchDashboardData() async throws -> HeartWatchDashboardData {
        async let latest: LatestHeartRateRecord? = fetchOptional("/api/latest")
        async let samples: [HeartRateSampleRecord] = fetchArray("/api/samples")
        async let events: [AvatarEventRecord] = fetchArray("/api/events")
        async let chat: [ChatMessageRecord] = fetchArray("/api/chat")

        return try await HeartWatchDashboardData(
            latest: latest,
            samples: samples,
            events: events,
            chatMessages: chat
        )
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

    private func endpoint(_ path: String) -> URL {
        URL(string: path, relativeTo: baseURL)!.absoluteURL
    }
}
