import React from "react";
import { createRoot } from "react-dom/client";
import { Apple, CheckCircle2, Database, Gamepad2, GitBranch, HeartPulse, PlayCircle, Smartphone, Watch } from "lucide-react";
import "./styles.css";

const setupSteps = [
  "Run scripts/start-demo-macos.command or scripts/start-demo-windows.ps1.",
  "Set the iPhone app API URL to the local network address printed by the startup script.",
  "Press the Quest 3 right-controller B button to start the demo.",
  "Tap Start on Apple Watch to begin live heart-rate capture.",
  "Use the iPhone app to view live heart rate, SQLite tables, VR events, and dialogue records.",
  "Press the Quest 3 right-controller menu button to exit VR and stop live writes."
];

const developerModeSteps = [
  "Connect iPhone to the Mac and run the app once from Xcode.",
  "Open Settings > Privacy & Security > Developer Mode.",
  "Turn Developer Mode on, restart the phone, and confirm after reboot.",
  "If the app is blocked, open Settings > General > VPN & Device Management and trust your developer account."
];

const endpoints = [
  ["GET", "/api/demo/status", "Read demo running state, latest heart rate, latest event, and latest dialogue"],
  ["POST", "/api/demo/start", "Quest B button starts the demo"],
  ["POST", "/api/demo/stop", "VR exit stops live writes"],
  ["POST", "/api/samples", "Apple Watch heart-rate sample"],
  ["GET", "/api/latest", "Unity VR heart-rate panel"],
  ["GET", "/api/samples", "iPhone heart-rate chart and history"],
  ["GET", "/api/db/tables", "iPhone SQLite table browser"],
  ["GET", "/api/events", "Avatar messages and VR events"],
  ["POST", "/api/chat/records", "Unity scripted dialogue records"],
  ["GET", "/api/chat", "iPhone dialogue table"]
];

export default function App() {
  return (
    <main className="app-shell">
      <section className="hero">
        <div className="hero-copy">
          <p className="eyebrow">Quest 3 + Apple Watch + iPhone + SQLite</p>
          <h1>VR Avatar HeartWatch Demo</h1>
          <p className="lead">
            A lightweight showcase app where Apple Watch captures heart rate, iPhone forwards samples to a SQLite backend,
            Quest 3 reads the same data in VR, and avatar dialogue plus exit events are written back to the database.
            The iPhone app displays charts, table structure, VR events, and dialogue records.
          </p>
        </div>
        <div className="system-card" aria-label="System architecture">
          <FlowItem icon={<Watch />} label="Apple Watch" detail="Live heart-rate capture" />
          <FlowItem icon={<Smartphone />} label="iPhone App" detail="Bridge, charts, records" />
          <FlowItem icon={<Database />} label="SQLite API" detail="Local backend and database" />
          <FlowItem icon={<Gamepad2 />} label="Quest 3 VR" detail="Avatar scene and panels" />
        </div>
      </section>

      <section className="content-grid">
        <InfoPanel title="Current Scope" icon={<GitBranch />}>
          <ul className="clean-list">
            <li><strong>VR:</strong> Unity Quest 3 scene with Robot Kyle, heart-rate panel, dialogue panel, start control, and exit control.</li>
            <li><strong>Sensor:</strong> Apple Watch captures live heart rate through a HealthKit workout.</li>
            <li><strong>Application:</strong> iPhone SwiftUI app handles bridging, charts, database browsing, VR events, and dialogue records.</li>
            <li><strong>Database:</strong> Express API + SQLite stores heart rate, avatar messages, VR events, and dialogue.</li>
          </ul>
        </InfoPanel>

        <InfoPanel title="Start Demo" icon={<PlayCircle />}>
          <ol className="step-list">
            {setupSteps.map((step) => <li key={step}>{step}</li>)}
          </ol>
        </InfoPanel>
      </section>

      <section className="docs-section">
        <InfoPanel title="Local Backend" icon={<Database />}>
          <p>Start the local SQLite backend with one file during the demo:</p>
          <CodeBlock code={"./scripts/start-demo-macos.command\n# Windows\n.\\scripts\\start-demo-windows.ps1"} />
          <p>Use the computer's local network address on iPhone and Quest 3, not localhost:</p>
          <CodeBlock code={"http://YOUR_MAC_IP:8787"} />
        </InfoPanel>

        <InfoPanel title="Install on iPhone" icon={<Apple />}>
          <ol className="step-list">
            <li>Open <code>ios/HeartWatchDemo/HeartWatchDemo.xcodeproj</code>.</li>
            <li>Select your own Apple Developer Team in Xcode.</li>
            <li>Select iPhone as the run device and click Run.</li>
            <li>The Watch app installs on the paired Apple Watch with the iPhone app.</li>
            <li>If iPhone blocks the app, trust your developer account in Settings.</li>
          </ol>
        </InfoPanel>
      </section>

      <section className="docs-section">
        <InfoPanel title="Developer Mode" icon={<Smartphone />}>
          <ol className="step-list">
            {developerModeSteps.map((step) => <li key={step}>{step}</li>)}
          </ol>
        </InfoPanel>

        <InfoPanel title="Project Targets" icon={<GitBranch />}>
          <ul className="clean-list">
            <li>iPhone target: bridge app, main interface, WatchConnectivity, data models, and API client.</li>
            <li>Watch target: Watch app, Watch interface, and HealthKit heart-rate manager.</li>
            <li>The Watch target requires HealthKit; iPhone and Watch both require WatchConnectivity.</li>
            <li>The iPhone app uses Swift Charts, so it requires iOS 16 or newer.</li>
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
            <li><code>GET /api/demo/status</code> returns running state, latest sample, latest event, and latest dialogue.</li>
            <li>The Quest right-controller B button writes a <code>demo_start</code> event.</li>
            <li>After Apple Watch starts, the iPhone app refreshes heart rate and database records every 3 seconds.</li>
            <li>The Unity heart-rate panel updates from <code>/api/latest</code>.</li>
            <li>The Quest right-controller menu button writes <code>demo_stop</code> and stops live writes.</li>
            <li>A/X/Y dialogue controls write records that are visible in the iPhone app.</li>
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
