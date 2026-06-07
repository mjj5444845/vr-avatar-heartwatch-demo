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
  message_type TEXT NOT NULL DEFAULT 'user_speech',
  conversation_initiator TEXT NOT NULL DEFAULT 'user',
  heart_rate INTEGER,
  zone TEXT,
  timestamp TEXT NOT NULL
);
