export function getHeartRateZone(heartRate) {
  if (heartRate < 70) {
    return { name: "calm", color: "#24b47e", tone: "steady" };
  }
  if (heartRate < 95) {
    return { name: "active", color: "#d8a800", tone: "engaged" };
  }
  if (heartRate < 120) {
    return { name: "elevated", color: "#f97316", tone: "supportive" };
  }
  return { name: "high", color: "#e5484d", tone: "grounding" };
}

export function createAvatarResponse(sample) {
  const zone = getHeartRateZone(sample.heartRate);
  const textByZone = {
    calm: "Your rhythm is steady. I can keep the VR scene open and exploratory.",
    active: "Your heart rate is active. I will keep the interaction responsive but comfortable.",
    elevated: "Your heart rate is elevated. I am slowing the pace and checking in with you.",
    high: "Your heart rate is high. I am pausing the session and guiding a grounding breath."
  };

  return {
    id: crypto.randomUUID(),
    type: "avatar_message",
    zone: zone.name,
    tone: zone.tone,
    text: textByZone[zone.name],
    timestamp: sample.timestamp
  };
}

