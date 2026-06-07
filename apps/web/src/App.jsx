import React, { useEffect, useMemo, useState } from "react";
import { Activity, Database, Headset, HeartPulse, Play, Square, Watch } from "lucide-react";
import { createAvatarResponse, getHeartRateZone } from "./avatarEngine.js";
import { createMockSample } from "./sensorMock.js";
import "./styles.css";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "";

export default function App() {
  const [samples, setSamples] = useState([]);
  const [events, setEvents] = useState([]);
  const [streaming, setStreaming] = useState(false);
  const latest = samples[0];
  const latestZone = latest ? getHeartRateZone(latest.heartRate) : null;
  const avatarMessage = latest ? createAvatarResponse(latest) : null;

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    if (!streaming) return undefined;

    const push = async () => {
      const sample = createMockSample();
      setSamples((current) => [sample, ...current].slice(0, 80));
      setEvents((current) => [createAvatarResponse(sample), ...current].slice(0, 80));

      if (API_BASE_URL) {
        await fetch(`${API_BASE_URL}/api/samples`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(sample)
        });
      }
    };

    push();
    const timer = window.setInterval(push, 2200);
    return () => window.clearInterval(timer);
  }, [streaming]);

  async function loadData() {
    if (!API_BASE_URL) return;

    const [sampleResponse, eventResponse] = await Promise.all([
      fetch(`${API_BASE_URL}/api/samples`),
      fetch(`${API_BASE_URL}/api/events`)
    ]);
    setSamples(await sampleResponse.json());
    setEvents(await eventResponse.json());
  }

  async function logVrEvent() {
    const event = {
      id: crypto.randomUUID(),
      type: "vr_focus",
      text: "Quest 3 user focused on avatar and requested a check-in.",
      timestamp: new Date().toISOString()
    };

    setEvents((current) => [event, ...current].slice(0, 80));
    if (API_BASE_URL) {
      await fetch(`${API_BASE_URL}/api/events`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(event)
      });
    }
  }

  const chartPath = useMemo(() => buildChartPath(samples), [samples]);

  return (
    <main className="app">
      <header className="topbar">
        <div>
          <p className="eyebrow">Unity Quest 3 + Apple Watch + SQLite</p>
          <h1>VR avatar heart-rate dashboard</h1>
        </div>
        <div className="actions">
          <button className="primary" onClick={() => setStreaming((value) => !value)}>
            {streaming ? <Square size={18} /> : <Play size={18} />}
            {streaming ? "Stop mock" : "Start mock"}
          </button>
          <button onClick={logVrEvent}>
            <Headset size={18} />
            VR event
          </button>
          <button onClick={loadData}>
            <Database size={18} />
            Sync API
          </button>
        </div>
      </header>

      <section className="system-grid">
        <StatusCard icon={<Watch />} label="Sensor" value={API_BASE_URL ? "Apple Watch/API ready" : "Mock mode"} />
        <StatusCard icon={<Headset />} label="VR" value="Unity Quest 3" />
        <StatusCard icon={<Database />} label="Database" value={API_BASE_URL ? "SQLite API" : "Local preview"} />
      </section>

      <section className="hero-grid">
        <article className="avatar-stage" style={{ "--zone": latestZone?.color || "#5f7cff" }}>
          <div className="avatar">
            <div className="avatar-aura" />
            <div className="avatar-head" />
            <div className="avatar-body" />
          </div>
          <div className="speech">
            <strong>Avatar</strong>
            <p>{avatarMessage?.text || "Waiting for Apple Watch heart-rate data."}</p>
          </div>
        </article>

        <aside className="live-panel">
          <span className="status">{streaming ? "Streaming mock heart rate" : "Waiting for live data"}</span>
          <div className="metric">
            <HeartPulse size={32} />
            <span>{latest?.heartRate || "--"}</span>
            <small>bpm</small>
          </div>
          <p>{latestZone ? `${latestZone.name} zone · ${latestZone.tone}` : "No current sample"}</p>
          <svg className="chart" viewBox="0 0 520 180" role="img" aria-label="Heart-rate chart">
            <line x1="0" x2="520" y1="96" y2="96" />
            <path d={chartPath} />
          </svg>
        </aside>
      </section>

      <section className="dashboard-grid">
        <article>
          <h2>Heart-rate records</h2>
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Time</th>
                  <th>Heart rate</th>
                  <th>Zone</th>
                  <th>Source</th>
                </tr>
              </thead>
              <tbody>
                {samples.slice(0, 20).map((sample) => {
                  const zone = getHeartRateZone(sample.heartRate);
                  return (
                    <tr key={sample.id}>
                      <td>{formatTime(sample.timestamp)}</td>
                      <td>{sample.heartRate} bpm</td>
                      <td>{zone.name}</td>
                      <td>{sample.source}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </article>

        <article>
          <h2>Avatar and VR events</h2>
          <div className="event-feed">
            {events.slice(0, 20).map((event) => (
              <div className="event-item" key={event.id}>
                <Activity size={18} />
                <div>
                  <strong>{event.text}</strong>
                  <span>{formatTime(event.timestamp)}{event.zone ? ` · ${event.zone}` : ""}</span>
                </div>
              </div>
            ))}
          </div>
        </article>
      </section>
    </main>
  );
}

function StatusCard({ icon, label, value }) {
  return (
    <article className="status-card">
      {icon}
      <div>
        <span>{label}</span>
        <strong>{value}</strong>
      </div>
    </article>
  );
}

function buildChartPath(samples) {
  const points = samples.slice(0, 32).reverse();
  if (points.length < 2) return "";

  return points.map((sample, index) => {
    const x = (index / (points.length - 1)) * 520;
    const y = 180 - ((sample.heartRate - 50) / 100) * 180;
    return `${index === 0 ? "M" : "L"} ${x.toFixed(1)} ${Math.max(12, Math.min(168, y)).toFixed(1)}`;
  }).join(" ");
}

function formatTime(value) {
  return new Intl.DateTimeFormat(undefined, {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit"
  }).format(new Date(value));
}

