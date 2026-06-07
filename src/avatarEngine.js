export function getHeartRateZone(heartRate) {
  if (heartRate < 70) {
    return { name: "calm", color: "#34c759", tone: "steady" };
  }

  if (heartRate < 95) {
    return { name: "active", color: "#ffcc00", tone: "engaged" };
  }

  if (heartRate < 120) {
    return { name: "elevated", color: "#ff9500", tone: "supportive" };
  }

  return { name: "high", color: "#ff3b30", tone: "grounding" };
}

export function createAvatarResponse(sample) {
  const zone = getHeartRateZone(sample.heartRate);
  const messages = {
    calm: "Your rhythm is steady. Want to explore the next scene together?",
    active: "I can feel your energy rising. I will keep the pace comfortable.",
    elevated: "Your heart rate is elevated. Let's slow the interaction and take one deeper breath.",
    high: "Your heart rate is high. I am pausing the session and guiding you back to a calmer state."
  };

  return {
    id: crypto.randomUUID(),
    timestamp: sample.timestamp,
    zone: zone.name,
    tone: zone.tone,
    text: messages[zone.name]
  };
}

