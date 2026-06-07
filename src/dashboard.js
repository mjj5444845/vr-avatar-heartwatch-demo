import { getHeartRateZone } from "./avatarEngine.js";

export function renderDashboard(state) {
  renderSamples(state.samples);
  renderEvents([...state.messages, ...state.events].sort(sortNewest));
  renderChart(state.samples);
}

export function renderLive(sample, message) {
  const zone = getHeartRateZone(sample.heartRate);
  document.querySelector("#heartRateValue").textContent = sample.heartRate;
  document.querySelector("#zoneLabel").textContent = `${zone.name} zone`;
  document.querySelector("#avatarMessage").textContent = message.text;
}

export function setSensorStatus(isRunning) {
  document.querySelector("#sensorStatus").textContent = isRunning ? "Sensor streaming" : "Sensor idle";
  document.querySelector("#sensorToggle").textContent = isRunning ? "Stop sensor" : "Start sensor";
}

function renderSamples(samples) {
  const rows = samples.slice(0, 18).map((sample) => {
    const zone = getHeartRateZone(sample.heartRate);
    return `
      <tr>
        <td>${formatTime(sample.timestamp)}</td>
        <td>${sample.heartRate} bpm</td>
        <td>${zone.name}</td>
        <td>${sample.source}</td>
      </tr>
    `;
  });

  document.querySelector("#sampleRows").innerHTML = rows.join("") || emptyRow();
}

function renderEvents(events) {
  const items = events.slice(0, 20).map((event) => `
    <div class="event-item">
      <strong>${event.text || event.type}</strong>
      <span>${formatTime(event.timestamp)}${event.zone ? ` · ${event.zone}` : ""}</span>
    </div>
  `);

  document.querySelector("#eventFeed").innerHTML = items.join("") || "<p>No events yet.</p>";
}

function renderChart(samples) {
  const points = samples.slice(0, 28).reverse();
  if (points.length < 2) {
    document.querySelector("#chart").innerHTML = "";
    return;
  }

  const width = 360;
  const height = 150;
  const min = 50;
  const max = 145;
  const path = points.map((sample, index) => {
    const x = (index / (points.length - 1)) * width;
    const y = height - ((sample.heartRate - min) / (max - min)) * height;
    return `${index === 0 ? "M" : "L"} ${x.toFixed(1)} ${Math.max(8, Math.min(height - 8, y)).toFixed(1)}`;
  }).join(" ");

  document.querySelector("#chart").innerHTML = `
    <svg viewBox="0 0 ${width} ${height}" role="img">
      <path d="${path}" fill="none" stroke="#1f6feb" stroke-width="4" stroke-linecap="round" />
      <line x1="0" y1="80" x2="${width}" y2="80" stroke="#c8d3e0" stroke-dasharray="5 6" />
    </svg>
  `;
}

function emptyRow() {
  return '<tr><td colspan="4">No heart-rate samples yet.</td></tr>';
}

function formatTime(value) {
  return new Intl.DateTimeFormat(undefined, {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit"
  }).format(new Date(value));
}

function sortNewest(a, b) {
  return new Date(b.timestamp) - new Date(a.timestamp);
}

