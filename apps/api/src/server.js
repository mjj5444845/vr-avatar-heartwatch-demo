import cors from "cors";
import express from "express";
import { createAvatarMessage, db, getZone } from "./db.js";

const app = express();
const port = Number(process.env.PORT || 8787);

app.use(cors());
app.use(express.json());

app.get("/api/health", (_request, response) => {
  response.json({ ok: true, database: "sqlite" });
});

app.post("/api/samples", (request, response) => {
  const sample = normalizeSample(request.body);
  const message = createAvatarMessage(sample);

  const insertSample = db.prepare(`
    INSERT OR REPLACE INTO heart_rate_samples (id, source, heart_rate, timestamp)
    VALUES (@id, @source, @heartRate, @timestamp)
  `);
  const insertMessage = db.prepare(`
    INSERT OR REPLACE INTO avatar_messages (id, sample_id, zone, tone, text, timestamp)
    VALUES (@id, @sampleId, @zone, @tone, @text, @timestamp)
  `);

  db.transaction(() => {
    insertSample.run(sample);
    insertMessage.run(message);
  })();

  response.status(201).json({ sample, message });
});

app.get("/api/samples", (_request, response) => {
  const rows = db.prepare(`
    SELECT id, source, heart_rate AS heartRate, timestamp
    FROM heart_rate_samples
    ORDER BY timestamp DESC
    LIMIT 120
  `).all();
  response.json(rows);
});

app.get("/api/latest", (_request, response) => {
  const sample = db.prepare(`
    SELECT id, source, heart_rate AS heartRate, timestamp
    FROM heart_rate_samples
    ORDER BY timestamp DESC
    LIMIT 1
  `).get();

  response.json(sample ? { ...sample, zone: getZone(sample.heartRate) } : null);
});

app.post("/api/events", (request, response) => {
  const event = {
    id: request.body.id || crypto.randomUUID(),
    type: request.body.type || "vr_event",
    text: request.body.text || "VR event",
    timestamp: request.body.timestamp || new Date().toISOString()
  };

  db.prepare(`
    INSERT OR REPLACE INTO vr_events (id, type, text, timestamp)
    VALUES (@id, @type, @text, @timestamp)
  `).run(event);

  response.status(201).json(event);
});

app.get("/api/events", (_request, response) => {
  const messages = db.prepare(`
    SELECT id, 'avatar_message' AS type, text, zone, timestamp
    FROM avatar_messages
    ORDER BY timestamp DESC
    LIMIT 80
  `).all();
  const vrEvents = db.prepare(`
    SELECT id, type, text, NULL AS zone, timestamp
    FROM vr_events
    ORDER BY timestamp DESC
    LIMIT 80
  `).all();

  response.json([...messages, ...vrEvents].sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp)).slice(0, 120));
});

app.post("/api/chat", (request, response) => {
  const transcript = String(request.body.text || request.body.transcript || "").trim();
  if (!transcript) {
    response.status(400).json({ error: "text is required" });
    return;
  }

  const messageType = normalizeMessageType(request.body.messageType || request.body.message_type);
  const conversationInitiator = normalizeConversationInitiator(request.body.conversationInitiator || request.body.conversation_initiator);
  const latest = db.prepare(`
    SELECT heart_rate AS heartRate
    FROM heart_rate_samples
    ORDER BY timestamp DESC
    LIMIT 1
  `).get();
  const zone = latest ? getZone(latest.heartRate) : { name: "unknown", tone: "neutral" };
  const userMessage = {
    id: crypto.randomUUID(),
    role: "user",
    text: transcript,
    messageType,
    conversationInitiator,
    heartRate: latest?.heartRate || null,
    zone: zone.name,
    timestamp: new Date().toISOString()
  };
  const assistantMessage = {
    id: crypto.randomUUID(),
    role: "avatar",
    text: createChatReply(transcript, latest?.heartRate, zone),
    messageType: "avatar_reply",
    conversationInitiator,
    heartRate: latest?.heartRate || null,
    zone: zone.name,
    timestamp: new Date().toISOString()
  };

  const insert = db.prepare(`
    INSERT INTO chat_messages (id, role, text, message_type, conversation_initiator, heart_rate, zone, timestamp)
    VALUES (@id, @role, @text, @messageType, @conversationInitiator, @heartRate, @zone, @timestamp)
  `);
  db.transaction(() => {
    insert.run(userMessage);
    insert.run(assistantMessage);
  })();

  response.json({
    transcript,
    reply: assistantMessage.text,
    messageType,
    conversationInitiator,
    heartRate: latest?.heartRate || null,
    zone
  });
});

app.post("/api/chat/records", (request, response) => {
  const text = String(request.body.text || "").trim();
  if (!text) {
    response.status(400).json({ error: "text is required" });
    return;
  }

  const explicitHeartRate = Number(request.body.heartRate || request.body.heart_rate);
  const latest = Number.isFinite(explicitHeartRate) && explicitHeartRate > 0
    ? { heartRate: Math.round(explicitHeartRate) }
    : db.prepare(`
      SELECT heart_rate AS heartRate
      FROM heart_rate_samples
      ORDER BY timestamp DESC
      LIMIT 1
    `).get();
  const zone = latest ? getZone(latest.heartRate) : { name: request.body.zone || "unknown", tone: "neutral" };
  const record = {
    id: request.body.id || crypto.randomUUID(),
    role: normalizeRole(request.body.role),
    text,
    messageType: normalizeMessageType(request.body.messageType || request.body.message_type),
    conversationInitiator: normalizeConversationInitiator(request.body.conversationInitiator || request.body.conversation_initiator),
    heartRate: latest?.heartRate || null,
    zone: request.body.zone || zone.name,
    timestamp: request.body.timestamp || new Date().toISOString()
  };

  db.prepare(`
    INSERT INTO chat_messages (id, role, text, message_type, conversation_initiator, heart_rate, zone, timestamp)
    VALUES (@id, @role, @text, @messageType, @conversationInitiator, @heartRate, @zone, @timestamp)
  `).run(record);

  response.status(201).json(record);
});

app.get("/api/chat", (_request, response) => {
  const rows = db.prepare(`
    SELECT
      id,
      role,
      text,
      message_type AS messageType,
      conversation_initiator AS conversationInitiator,
      heart_rate AS heartRate,
      zone,
      timestamp
    FROM chat_messages
    ORDER BY timestamp DESC
    LIMIT 80
  `).all();

  response.json(rows);
});

app.listen(port, () => {
  console.log(`SQLite API listening on http://localhost:${port}`);
});

function normalizeSample(body) {
  const heartRate = Number(body.heartRate || body.heart_rate);
  if (!Number.isFinite(heartRate)) {
    throw new Error("heartRate is required");
  }

  return {
    id: body.id || crypto.randomUUID(),
    source: body.source || "apple_watch",
    heartRate: Math.round(heartRate),
    timestamp: body.timestamp || new Date().toISOString()
  };
}

function createChatReply(transcript, heartRate, zone) {
  const state = heartRate ? `Your current heart rate is ${heartRate} bpm, in the ${zone.name} zone.` : "I do not have a heart-rate sample yet.";
  const lower = transcript.toLowerCase();

  if (lower.includes("pause") || lower.includes("stop")) {
    return `${state} I will pause the VR interaction and stay with you while the scene settles.`;
  }
  if (lower.includes("breath") || lower.includes("calm")) {
    return `${state} Let's take a slow breath together: inhale for four, hold for two, exhale for six.`;
  }
  if (lower.includes("how") && lower.includes("feel")) {
    return `${state} I am reading your signal as ${zone.tone}. Tell me if you want the room quieter or more active.`;
  }

  return `${state} I heard: "${transcript}". I will adapt the avatar response to your current state.`;
}

function normalizeMessageType(value) {
  const allowed = new Set(["user_speech", "avatar_reply", "sensor_prompt", "system"]);
  return allowed.has(value) ? value : "user_speech";
}

function normalizeRole(value) {
  const allowed = new Set(["user", "avatar", "system"]);
  return allowed.has(value) ? value : "avatar";
}

function normalizeConversationInitiator(value) {
  const allowed = new Set(["user", "avatar"]);
  return allowed.has(value) ? value : "user";
}
