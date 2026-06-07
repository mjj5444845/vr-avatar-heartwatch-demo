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

