export function updateAvatarScene(message, zone) {
  const aura = document.querySelector("#avatarAura");
  const head = document.querySelector("#avatarHead");
  const speech = document.querySelector("#avatarSpeech");

  if (!aura || !head || !speech) {
    return;
  }

  aura.setAttribute("color", zone.color);
  head.setAttribute("color", zone.color);
  speech.setAttribute("value", message.text);

  const scale = zone.name === "high" ? "1.12 1.12 1.12" : "1 1 1";
  aura.setAttribute("scale", scale);
}

