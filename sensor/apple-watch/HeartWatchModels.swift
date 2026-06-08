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
    var databaseTables: [DatabaseTableRecord] = []
}

struct DatabaseSummaryRecord: Codable {
    let ok: Bool
    let samples: Int
    let events: Int
    let chat: Int
    let latest: LatestHeartRateRecord?
}

struct DatabaseTablesResponse: Codable {
    let tables: [DatabaseTableRecord]
}

struct DatabaseTableRecord: Identifiable, Codable {
    var id: String { name }

    let name: String
    let columns: [DatabaseColumnRecord]
    let rowCount: Int
    let rows: [[String: JSONValue]]
}

struct DatabaseColumnRecord: Identifiable, Codable {
    var id: String { name }

    let name: String
    let type: String
    let required: Bool
    let primaryKey: Bool
    let defaultValue: JSONValue?
}

enum JSONValue: Codable, CustomStringConvertible {
    case string(String)
    case number(Double)
    case bool(Bool)
    case null

    init(from decoder: Decoder) throws {
        let container = try decoder.singleValueContainer()
        if container.decodeNil() {
            self = .null
        } else if let value = try? container.decode(Bool.self) {
            self = .bool(value)
        } else if let value = try? container.decode(Double.self) {
            self = .number(value)
        } else {
            self = .string((try? container.decode(String.self)) ?? "")
        }
    }

    func encode(to encoder: Encoder) throws {
        var container = encoder.singleValueContainer()
        switch self {
        case .string(let value):
            try container.encode(value)
        case .number(let value):
            try container.encode(value)
        case .bool(let value):
            try container.encode(value)
        case .null:
            try container.encodeNil()
        }
    }

    var description: String {
        switch self {
        case .string(let value):
            return value
        case .number(let value):
            return value.rounded() == value ? String(Int(value)) : String(value)
        case .bool(let value):
            return value ? "true" : "false"
        case .null:
            return "--"
        }
    }
}
