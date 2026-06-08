# Windows Pipeline Troubleshooting

Use this checklist when the full demo does not connect on a Windows laptop.

The USB cable between iPhone and Windows is only useful for charging or device management. The HeartWatch iPhone app sends data to the Windows backend over the network. The iPhone and Windows laptop must be able to reach each other on Wi-Fi or through a Windows mobile hotspot.

## What to Enter on iPhone

Run:

```powershell
scripts\start-demo-windows.ps1
```

The launcher prints a section named:

```text
HeartWatch iPhone app connection value:
  http://WINDOWS_WIFI_IP:8787
```

Enter exactly that base URL in the iPhone app Settings screen.

Use:

```text
http://WINDOWS_WIFI_IP:8787
```

Do not enter:

```text
http://WINDOWS_WIFI_IP:8787/api/health
http://WINDOWS_WIFI_IP:8787/api/samples
127.0.0.1
localhost
```

On iPhone, `127.0.0.1` and `localhost` mean the iPhone itself, not the Windows laptop.

## First Test: iPhone to Windows API

Before opening the HeartWatch iPhone app, open Safari on iPhone and visit:

```text
http://WINDOWS_WIFI_IP:8787/api/health
```

Expected result:

```json
{"ok":true,"database":"sqlite"}
```

If Safari cannot open this URL, the iPhone app cannot connect either. Fix this before testing Apple Watch.

Common causes:

- Windows Firewall is blocking inbound port `8787`.
- PowerShell launcher was not run as Administrator, so it could not add the firewall rule.
- iPhone and Windows are on different networks.
- The Wi-Fi network uses guest mode, client isolation, school/corporate isolation, or VPN routing.
- The launcher printed an Ethernet, VM, VPN, Docker, or hotspot adapter IP instead of the active Wi-Fi IP.

Manual firewall command for Administrator PowerShell:

```powershell
New-NetFirewallRule -DisplayName 'VR Avatar HeartWatch API 8787' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8787
```

If the normal Wi-Fi blocks device-to-device traffic, use Windows mobile hotspot:

1. On Windows, open Settings.
2. Enable Mobile hotspot.
3. Connect iPhone to that hotspot Wi-Fi.
4. Run `scripts\start-demo-windows.ps1` again.
5. Use the newly printed `http://WINDOWS_WIFI_IP:8787` value.

## Second Test: iPhone App to Windows API

In the HeartWatch iPhone app:

1. Open Settings.
2. Enter `http://WINDOWS_WIFI_IP:8787`.
3. Tap Save and Test.
4. Confirm API shows Online.
5. Open the Data tab.
6. Confirm SQLite tables are visible.

If Safari works but the app does not, check:

- The app Settings value must be the base URL only.
- The URL must start with `http://`, not `https://`.
- The app must be rebuilt and reinstalled after code changes.
- Local Network permission may be required by iOS. If prompted, allow it.

## Third Test: Watch to iPhone

Keep the iPhone app open in the foreground.

On Apple Watch:

1. Open HeartWatch.
2. Tap Start.
3. Wait for the watch screen to show live heart rate.
4. Watch the iPhone Overview page.

Expected iPhone status:

```text
Watch sample received: 80 bpm
```

If the iPhone shows Watch samples but the database count does not increase, the Watch-to-iPhone link works. The remaining issue is demo state or API writing.

If the iPhone never shows Watch samples:

- Confirm the iPhone app and Watch app were installed from the same Xcode project and same bundle family.
- Keep both apps open during the first test.
- Confirm the Apple Watch is paired to this iPhone.
- Restart both apps after installation.
- In Xcode, install both the iPhone target and the Watch target again.

## Fourth Test: VR Starts Recording

The iPhone app only writes live Watch samples into SQLite while the demo backend says the VR demo is running.

Start order:

1. Run `scripts\start-demo-windows.ps1`.
2. Enter the printed base URL in the iPhone app.
3. Verify iPhone Safari can open `/api/health`.
4. Open the Windows VR app.
5. Press Quest right-hand B to start the demo.
6. Start HeartWatch on Apple Watch.

Expected API log:

```text
POST /api/demo/start 200
POST /api/samples 201
GET /api/latest 200
```

If Watch samples are received on iPhone but API logs do not show `POST /api/samples`, check whether the iPhone Settings page says the demo is Running. If it says Stopped, press Quest right-hand B again.

## Where to Look

Runtime logs are written under:

```text
logs/
```

Important files:

- `demo-run-*.log`: launcher output, printed IP addresses, firewall status, Unity launch.
- `api-output-*.log`: API startup and every HTTP request.
- `api-error-*.log`: Windows API stderr output.

Useful API checks:

```text
http://WINDOWS_WIFI_IP:8787/api/health
http://WINDOWS_WIFI_IP:8787/api/demo/status
http://WINDOWS_WIFI_IP:8787/api/latest
http://WINDOWS_WIFI_IP:8787/api/db/tables
```

