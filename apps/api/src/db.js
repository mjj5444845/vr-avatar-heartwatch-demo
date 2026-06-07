import Database from "better-sqlite3";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const dataDir = path.resolve(__dirname, "../data");
const dbPath = process.env.SQLITE_PATH || path.join(dataDir, "demo.sqlite");

fs.mkdirSync(dataDir, { recursive: true });

export const db = new Database(dbPath);
db.pragma("journal_mode = WAL");

db.exec(`
  CREATE TABLE IF NOT EXISTS heart_rate_samples (
    id TEXT PRIMARY KEY,
    source TEXT NOT NULL,
    heart_rate INTEGER NOT NULL,
    timestamp TEXT NOT NULL
  );

  CREATE TABLE IF NOT EXISTS avatar_messages (
    id TEXT PRIMARY KEY,
    sample_id TEXT,
    zone TEXT NOT NULL,
    tone TEXT NOT NULL,
    text TEXT NOT NULL,
    timestamp TEXT NOT NULL,
    FOREIGN KEY(sample_id) REFERENCES heart_rate_samples(id)
  );

  CREATE TABLE IF NOT EXISTS vr_events (
    id TEXT PRIMARY KEY,
    type TEXT NOT NULL,
    text TEXT NOT NULL,
    timestamp TEXT NOT NULL
  );

  CREATE TABLE IF NOT EXISTS chat_messages (
    id TEXT PRIMARY KEY,
    role TEXT NOT NULL,
    text TEXT NOT NULL,
    message_type TEXT NOT NULL DEFAULT 'spoken_text',
    conversation_initiator TEXT NOT NULL DEFAULT 'user',
    heart_rate INTEGER,
    zone TEXT,
    timestamp TEXT NOT NULL
  );
`);

ensureColumn("chat_messages", "message_type", "TEXT NOT NULL DEFAULT 'spoken_text'");
ensureColumn("chat_messages", "conversation_initiator", "TEXT NOT NULL DEFAULT 'user'");

function ensureColumn(tableName, columnName, definition) {
  const columns = db.prepare(`PRAGMA table_info(${tableName})`).all();
  if (!columns.some((column) => column.name === columnName)) {
    db.exec(`ALTER TABLE ${tableName} ADD COLUMN ${columnName} ${definition}`);
  }
}

export function getZone(heartRate) {
  if (heartRate < 70) return { name: "calm", tone: "steady" };
  if (heartRate < 95) return { name: "active", tone: "engaged" };
  if (heartRate < 120) return { name: "elevated", tone: "supportive" };
  return { name: "high", tone: "grounding" };
}

export function createAvatarMessage(sample) {
  const zone = getZone(sample.heartRate);
  const textByZone = {
    calm: "Your rhythm is steady. I can keep the VR scene open and exploratory.",
    active: "Your heart rate is active. I will keep the interaction responsive but comfortable.",
    elevated: "Your heart rate is elevated. I am slowing the pace and checking in with you.",
    high: "Your heart rate is high. I am pausing the session and guiding a grounding breath."
  };

  return {
    id: crypto.randomUUID(),
    sampleId: sample.id,
    zone: zone.name,
    tone: zone.tone,
    text: textByZone[zone.name],
    timestamp: sample.timestamp
  };
}
