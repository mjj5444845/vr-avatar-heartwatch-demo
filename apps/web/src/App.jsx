import React from "react";
import { createRoot } from "react-dom/client";
import { Apple, CheckCircle2, Database, Gamepad2, GitBranch, HeartPulse, PlayCircle, Smartphone, Watch } from "lucide-react";
import "./styles.css";

const setupSteps = [
  "Run scripts/start-demo-macos.command or scripts/start-demo-windows.ps1.",
  "Set the iPhone app API base URL to the printed computer LAN address.",
  "Start the Apple Watch heart-rate workout stream.",
  "Open the Unity Quest 3 scene or built app on the same network.",
  "Press the right-hand B button in Quest 3 to start the demo.",
  "Inspect live heart rate, SQLite tables, VR events, and conversation rows in the iPhone app."
];

const developerModeSteps = [
  "Connect iPhone to the Mac and try running the app once from Xcode.",
  "Open Settings > Privacy & Security > Developer Mode.",
  "Turn Developer Mode on, restart, then confirm Developer Mode after reboot.",
  "If the app is blocked, open Settings > General > VPN & Device Management and trust your developer account."
];

const endpoints = [
  ["GET", "/api/demo/status", "Single demo status for app and smoke tests"],
  ["POST", "/api/demo/start", "Quest B button demo-start event"],
  ["POST", "/api/samples", "Apple Watch heart-rate samples"],
  ["GET", "/api/latest", "Unity VR current heart-rate panel"],
  ["GET", "/api/samples", "iPhone chart and heart-rate history"],
  ["GET", "/api/db/tables", "iPhone SQLite table browser"],
  ["GET", "/api/events", "Avatar zone messages and VR events"],
  ["POST", "/api/chat/records", "Unity scripted conversation records"],
  ["GET", "/api/chat", "iPhone conversation table"]
];

export default function App() {
  return (
    <main className="app-shell">
      <section className="hero">
        <div className="hero-copy">
          <p className="eyebrow">Quest 3 + Apple Watch + iPhone + SQLite</p>
          <h1>VR Avatar HeartWatch Demo</h1>
          <p className="lead">
            A lightweight presentation app where Apple Watch heart rate flows into SQLite, Quest 3 reads the same
            backend in VR, and the iPhone app shows the live chart, database tables, VR events, and dialogue history.
          </p>
        </div>
        <div className="system-card" aria-label="System architecture">
          <FlowItem icon={<Watch />} label="Apple Watch" detail="HealthKit live heart rate" />
          <FlowItem icon={<Smartphone />} label="iPhone App" detail="Bridge, chart, records" />
          <FlowItem icon={<Database />} label="SQLite API" detail="Local durable demo data" />
          <FlowItem icon={<Gamepad2 />} label="Quest 3 VR" detail="Avatar scene and panels" />
        </div>
      </section>

      <section className="content-grid">
        <InfoPanel title="Current Scope" icon={<GitBranch />}>
          <ul className="clean-list">
            <li><strong>VR:</strong> Unity Quest 3 scene with Robot Kyle, heart-rate panel, dialogue panel, and scripted controls.</li>
            <li><strong>Sensor:</strong> Apple Watch HealthKit workout stream sent through WatchConnectivity.</li>
            <li><strong>Application:</strong> iPhone SwiftUI app for Watch bridge, live chart, records, events, and conversations.</li>
            <li><strong>Database:</strong> Express API with SQLite tables for samples, avatar messages, VR events, and chat messages.</li>
          </ul>
        </InfoPanel>

        <InfoPanel title="Start The Demo" icon={<PlayCircle />}>
          <ol className="step-list">
            {setupSteps.map((step) => <li key={step}>{step}</li>)}
          </ol>
        </InfoPanel>
      </section>

      <section className="docs-section">
        <InfoPanel title="Mac API" icon={<Database />}>
          <p>For a presentation, start the local SQLite backend with one file:</p>
          <CodeBlock code={"./scripts/start-demo-macos.command\n# or on Windows\n.\\scripts\\start-demo-windows.ps1"} />
          <p>For iPhone and Quest 3, use the Mac LAN address, not localhost:</p>
          <CodeBlock code={"http://YOUR_MAC_IP:8787"} />
        </InfoPanel>

        <InfoPanel title="Install On iPhone" icon={<Apple />}>
          <ol className="step-list">
            <li>Open Xcode and create an iOS app with a watchOS companion app.</li>
            <li>Add the iPhone Swift files from <code>sensor/apple-watch</code> to the iOS target.</li>
            <li>Add the Watch Swift files from <code>sensor/apple-watch</code> to the Watch target.</li>
            <li>Enable HealthKit on Watch and WatchConnectivity on both targets.</li>
            <li>Select your iPhone as the run destination and press Run.</li>
            <li>If prompted, trust the developer app on iPhone in Settings.</li>
          </ol>
        </InfoPanel>
      </section>

      <section className="docs-section">
        <InfoPanel title="Developer Mode" icon={<Smartphone />}>
          <ol className="step-list">
            {developerModeSteps.map((step) => <li key={step}>{step}</li>)}
          </ol>
        </InfoPanel>

        <InfoPanel title="Assemble Targets" icon={<GitBranch />}>
          <ul className="clean-list">
            <li>iPhone target: bridge app, bridge view, WatchConnectivity bridge, models, and API client.</li>
            <li>Watch target: Watch app, Watch view, and HealthKit heart-rate manager.</li>
            <li>Enable HealthKit on Watch and WatchConnectivity on both targets.</li>
            <li>Use iOS 16 or newer because the iPhone app uses Swift Charts.</li>
          </ul>
        </InfoPanel>
      </section>

      <section className="docs-section">
        <InfoPanel title="API Contract" icon={<HeartPulse />}>
          <div className="endpoint-list">
            {endpoints.map(([method, path, description]) => (
              <div className="endpoint" key={path}>
                <span>{method}</span>
                <code>{path}</code>
                <p>{description}</p>
              </div>
            ))}
          </div>
        </InfoPanel>

        <InfoPanel title="Test Checklist" icon={<CheckCircle2 />}>
          <ul className="clean-list">
            <li><code>GET /api/demo/status</code> returns the current backend, latest sample, latest event, and latest chat row.</li>
            <li>Watch app shows a current bpm value after Start and Health permission approval.</li>
            <li>iPhone app updates every 3 seconds while the API is online.</li>
            <li>Unity heart-rate panel updates from <code>/api/latest</code>.</li>
            <li>Quest right-hand B writes a demo-start event visible in the iPhone app.</li>
            <li>Unity A/X/Y dialogue controls create rows visible in the iPhone app conversation view.</li>
          </ul>
        </InfoPanel>
      </section>
    </main>
  );
}

function FlowItem({ icon, label, detail }) {
  return (
    <div className="flow-item">
      {icon}
      <div>
        <strong>{label}</strong>
        <span>{detail}</span>
      </div>
    </div>
  );
}

function InfoPanel({ title, icon, children }) {
  return (
    <article className="info-panel">
      <header>
        {icon}
        <h2>{title}</h2>
      </header>
      {children}
    </article>
  );
}

function CodeBlock({ code }) {
  return <pre><code>{code}</code></pre>;
}

createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
