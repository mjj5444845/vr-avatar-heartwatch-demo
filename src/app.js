import { createAvatarResponse, getHeartRateZone } from "./avatarEngine.js";
import { appendRecord, exportState, loadState, resetState } from "./database.js";
import { renderDashboard, renderLive, setSensorStatus } from "./dashboard.js";
import { isSensorRunning, startMockSensor, stopMockSensor } from "./sensorMock.js";
import { updateAvatarScene } from "./vrScene.js";

function boot() {
  renderDashboard(loadState());
  setSensorStatus(false);

  document.querySelector("#sensorToggle").addEventListener("click", toggleSensor);
  document.querySelector("#addVrEvent").addEventListener("click", logVrEvent);
  document.querySelector("#exportData").addEventListener("click", exportState);
  document.querySelector("#resetData").addEventListener("click", () => {
    stopMockSensor();
    renderDashboard(resetState());
    setSensorStatus(false);
    document.querySelector("#heartRateValue").textContent = "--";
    document.querySelector("#zoneLabel").textContent = "Waiting for Apple Watch data";
    document.querySelector("#avatarMessage").textContent = "Start the sensor to begin the session.";
  });
}

function toggleSensor() {
  if (isSensorRunning()) {
    stopMockSensor();
    setSensorStatus(false);
    return;
  }

  startMockSensor(handleSample);
  setSensorStatus(true);
}

function handleSample(sample) {
  const message = createAvatarResponse(sample);
  let state = appendRecord("samples", sample);
  state = appendRecord("messages", message);
  const zone = getHeartRateZone(sample.heartRate);
  renderLive(sample, message);
  renderDashboard(state);
  updateAvatarScene(message, zone);
}

function logVrEvent() {
  const event = {
    id: crypto.randomUUID(),
    type: "vr_interaction",
    text: "User focused on avatar and triggered a check-in.",
    timestamp: new Date().toISOString()
  };
  renderDashboard(appendRecord("events", event));
}

boot();

