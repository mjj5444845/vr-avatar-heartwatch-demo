let timer = null;
let current = 78;

function nextHeartRate() {
  const drift = Math.round((Math.random() - 0.42) * 14);
  const occasionalSpike = Math.random() > 0.88 ? Math.round(Math.random() * 22) : 0;
  current = Math.max(58, Math.min(136, current + drift + occasionalSpike));
  return current;
}

export function startMockSensor(onSample) {
  if (timer) {
    return;
  }

  onSample(createSample());
  timer = window.setInterval(() => onSample(createSample()), 2200);
}

export function stopMockSensor() {
  window.clearInterval(timer);
  timer = null;
}

export function isSensorRunning() {
  return Boolean(timer);
}

function createSample() {
  return {
    id: crypto.randomUUID(),
    source: "apple_watch_mock",
    heartRate: nextHeartRate(),
    timestamp: new Date().toISOString()
  };
}

