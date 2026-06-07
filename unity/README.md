# Unity Quest 3 Setup

This folder contains lightweight C# scripts to import into a Unity project. It is intentionally not a full Unity project because Unity generates large local folders that should not be committed.

## Create The Unity Project

1. Open **Unity Hub**.
2. Click **New project**.
3. Choose **3D URP** or **3D Core**.
4. Name it `VRAvatarHeartWatch`.
5. Put it outside this repo or inside `unity/VRAvatarHeartWatch` if you prefer.

## Quest 3 Build Settings

1. Install **Android Build Support** in Unity Hub for your Unity editor version.
2. In Unity, open **File > Build Profiles**.
3. Switch platform to **Android**.
4. Open **Edit > Project Settings > XR Plug-in Management**.
5. Install XR Plug-in Management if prompted.
6. Enable **OpenXR** under Android.
7. In **OpenXR**, enable Meta Quest support features if available.
8. Set **Player > Other Settings > Minimum API Level** to Android 10 or newer.

## Import Scripts

Copy `Assets/Scripts` from this repo into your Unity project's `Assets/Scripts` folder.

Add these scripts to GameObjects:

- `HeartRateReceiver.cs`: attach to an empty GameObject named `HeartRateReceiver`.
- `AvatarMoodController.cs`: attach to the avatar root or a controller object.

In the Unity Inspector:

- Set `HeartRateReceiver.apiBaseUrl` to your API URL, for example `http://192.168.1.20:8787`.
- Link `AvatarMoodController` to the `HeartRateReceiver`.
- Assign avatar renderers/materials to change color by heart-rate zone.

## Run On Quest 3

1. Enable Developer Mode for Quest 3 in the Meta mobile app.
2. Connect Quest 3 to the Mac with USB-C.
3. Allow USB debugging inside the headset.
4. In Unity, choose **Build And Run**.

The Quest 3 app must reach the API over the same network. Use your Mac LAN IP, not `localhost`, when running the SQLite API on your Mac.

