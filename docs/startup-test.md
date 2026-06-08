# Startup Test

Use this checklist when you want to confirm the whole demo is connected.

## 1. Start The Mac API

```bash
npm install
npm run api:dev
```

Find the Mac LAN IP on the same Wi-Fi as iPhone and Quest 3. Use this format in the iPhone app and Unity:

```text
http://YOUR_MAC_IP:8787
```

Check the API:

```bash
curl http://127.0.0.1:8787/api/health
curl http://127.0.0.1:8787/api/db/summary
curl http://127.0.0.1:8787/api/db/tables
```

## 2. Install And Run iPhone + Apple Watch

Open:

```text
ios/HeartWatchDemo/HeartWatchDemo.xcodeproj
```

In Xcode, set your own Apple Developer Team for both targets:

```text
HeartWatchDemo
HeartWatchDemoWatchApp
```

Run the iPhone app on iPhone and the Watch app on Apple Watch.

In the iPhone app:

1. Open **Settings**.
2. Set API URL to `http://YOUR_MAC_IP:8787`.
3. Tap **Save and Test**.

On Apple Watch:

1. Open **HeartWatch**.
2. Tap **Start**.
3. Allow **Heart Rate** and **Workouts** if prompted.

Back on iPhone:

1. Open **Overview**.
2. Tap **Sync**.
3. Confirm the bpm, source, and zone update.

## 3. Check Database Browser

In the iPhone app:

1. Open **Data**.
2. Confirm table names appear:
   - `heart_rate_samples`
   - `avatar_messages`
   - `vr_events`
   - `chat_messages`
3. Select a table.
4. Expand **Columns** and recent **Row** items.

## 4. Check VR Interfaces

In the iPhone app:

1. Open **VR Test**.
2. Tap **Post sample and dialogue test**.
3. Confirm the status says `/api/latest` returned bpm and `/api/chat/records` stored a row.
4. Open **Data** and inspect `heart_rate_samples` and `chat_messages`.

This tests the same API contract Unity uses:

```text
GET  /api/latest
POST /api/chat/records
GET  /api/chat
```

## 5. Check Unity / Quest 3

In Unity, open:

```text
unity/VRAvatarHeartWatch/Assets/Scenes/Quest3LivingRoomAvatar.unity
```

Set both API fields to the Mac LAN URL:

```text
HeartRateReceiver.apiBaseUrl = http://YOUR_MAC_IP:8787
ScriptedConversationController.apiBaseUrl = http://YOUR_MAC_IP:8787
```

Run in Editor or build to Quest 3.

Expected result:

1. The heart-rate panel reads the latest bpm from `/api/latest`.
2. Press `X` to start a dialogue.
3. Press `Y` to switch dialogue.
4. Press `A`, `N`, or Enter for next line.
5. The iPhone app **VR Test** or **Data** tab shows new conversation rows after Sync.

## Privacy Check

Before pushing public changes:

```bash
git ls-files | rg '(\\.env|secret|key|\\.p8|mobileprovision|sqlite|\\.db)'
rg -n -i '(api[_-]?key|secret|token|password|bearer |sk-|github_pat_|ghp_|DEVELOPMENT_TEAM)' --glob '!node_modules/**' --glob '!apps/api/data/**'
```

No personal Team ID, API key, token, SQLite database, provisioning profile, or `.env` file should be committed.
