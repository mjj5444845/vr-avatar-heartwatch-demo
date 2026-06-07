import Foundation

struct HeartRateSampleRecord: Identifiable, Codable {
    let id: String
    let source: String
    let heartRate: Int
    let timestamp: String
}

struct LatestHeartRateRecord: Identifiable, Codable {
    let id: String
    let source: String
    let heartRate: Int
    let timestamp: String
    let zone: HeartRateZone?
}

struct HeartRateZone: Codable {
    let name: String
    let tone: String
}

struct AvatarEventRecord: Identifiable, Codable {
    let id: String
    let type: String
    let text: String
    let zone: String?
    let timestamp: String
}

struct ChatMessageRecord: Identifiable, Codable {
    let id: String
    let role: String
    let text: String
    let messageType: String
    let conversationInitiator: String
    let heartRate: Int?
    let zone: String?
    let timestamp: String
}

struct HeartWatchDashboardData {
    var latest: LatestHeartRateRecord?
    var samples: [HeartRateSampleRecord] = []
    var events: [AvatarEventRecord] = []
    var chatMessages: [ChatMessageRecord] = []
    var databaseSummary: DatabaseSummaryRecord?
}

struct DatabaseSummaryRecord: Codable {
    let ok: Bool
    let samples: Int
    let events: Int
    let chat: Int
    let latest: LatestHeartRateRecord?
}
