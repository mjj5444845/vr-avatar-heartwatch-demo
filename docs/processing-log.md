# Processing Log

This file records what the demo system does at runtime and where to find the generated logs.

## Runtime Log Files

Each launcher creates a `logs` folder in the package root.

macOS:

```text
logs/demo-run-YYYYMMDD-HHMMSS.log
logs/api-output-YYYYMMDD-HHMMSS.log
```

Windows:

```text
logs/demo-run-YYYYMMDD-HHMMSS.log
logs/api-output-YYYYMMDD-HHMMSS.log
logs/api-error-YYYYMMDD-HHMMSS.log
```

The `demo-run` log records launcher actions:

- detected API URL candidates
- SQLite database path
- API process start
- local API health check
- LAN URL checks
- Unity app launch
- troubleshooting instructions

The `api-output` log records backend activity:

- API startup
- every HTTP request method and path
- HTTP status code
- request duration

Example API log line:

```text
2026-06-08T12:00:00.000Z POST /api/samples 201 4ms
```

## Runtime Sequence

1. The launcher starts the local SQLite API on `0.0.0.0:8787`.
2. The launcher prints one or more LAN URLs, such as `http://192.168.1.20:8787`.
3. The iPhone app uses that URL as its API base URL.
4. Apple Watch sends heart-rate samples to the iPhone app.
5. The iPhone app posts heart-rate samples to `POST /api/samples`.
6. SQLite stores each sample in `heart_rate_samples`.
7. Unity reads the latest sample from `GET /api/latest`.
8. Unity writes start/stop events to `POST /api/demo/start` and `POST /api/demo/stop`.
9. Unity writes scripted dialogue rows to `POST /api/chat/records`.
10. The iPhone app reads database details from `GET /api/db/tables`.

## Windows Connectivity Checklist

If the API URL printed on Windows does not work on iPhone:

1. Confirm iPhone and Windows are on the same Wi-Fi.
2. Open Safari on iPhone and visit `http://WINDOWS_IP:8787/api/health`.
3. If Safari cannot open it, Windows Firewall is probably blocking Node/npm.
4. Re-run PowerShell as Administrator and start the demo again. The script will try to create an inbound TCP firewall rule for port `8787`.
5. If the Windows computer has multiple network adapters, try each URL printed under "All detected LAN candidates".
6. Avoid VPN, guest Wi-Fi, hotspot client isolation, and corporate Wi-Fi networks that block device-to-device traffic.

## What Is Not Logged

The logs do not record raw Apple Health credentials, Apple account details, API keys, or private developer account data.

The logs do record heart-rate samples and demo interaction events because that is the purpose of the demo.

