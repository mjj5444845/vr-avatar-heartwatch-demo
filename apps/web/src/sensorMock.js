let current = 78;

export function createMockSample() {
  const drift = Math.round((Math.random() - 0.42) * 14);
  const spike = Math.random() > 0.88 ? Math.round(Math.random() * 24) : 0;
  current = Math.max(58, Math.min(138, current + drift + spike));

  return {
    id: crypto.randomUUID(),
    source: "apple_watch_mock",
    heartRate: current,
    timestamp: new Date().toISOString()
  };
}

