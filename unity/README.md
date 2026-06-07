# Unity Quest 3 Setup

This folder contains a lightweight Unity project shell plus reusable C# scripts.

The ready-to-open project folder is:

```text
unity/VRAvatarHeartWatch
```

If Unity reports that Rosetta 2 is missing, install it first:

```bash
softwareupdate --install-rosetta --agree-to-license
```

## Open The Unity Project

1. Open **Unity Hub**.
2. Click **Add**.
3. Select `unity/VRAvatarHeartWatch`.
4. Open it with Unity `6000.4.10f1`.
5. Run **VR Avatar Demo > Build Quest 3 Demo Scene**.

## Quest 3 Build Settings

1. Install **Android Build Support** in Unity Hub for your Unity editor version.
2. In Unity, open **File > Build Profiles**.
3. Switch platform to **Android**.
4. Open **Edit > Project Settings > XR Plug-in Management**.
5. Install XR Plug-in Management if prompted.
6. Enable **OpenXR** under Android.
7. In **OpenXR**, enable Meta Quest support features if available.
8. Set **Player > Other Settings > Minimum API Level** to Android 10 or newer.

## Generated Scene

The scene builder creates:

- room walls, floor, and lighting,
- a placeholder avatar,
- Quest 3 XR Origin, camera, and controller placeholders,
- a current heart-rate panel,
- an avatar reply dialogue panel,
- a heart-rate API poller,
- an LLM conversation bridge.

Set `HeartRateReceiver.apiBaseUrl` and `LlmConversationController.apiBaseUrl` to your Mac LAN IP, for example `http://192.168.1.20:8787`.

## Run On Quest 3

1. Enable Developer Mode for Quest 3 in the Meta mobile app.
2. Connect Quest 3 to the Mac with USB-C.
3. Allow USB debugging inside the headset.
4. In Unity, choose **Build And Run**.

The Quest 3 app must reach the API over the same network. Use your Mac LAN IP, not `localhost`, when running the SQLite API on your Mac.
