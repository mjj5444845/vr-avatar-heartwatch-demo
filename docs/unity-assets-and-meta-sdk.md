# Unity Assets And Meta SDK Setup

This project is prepared for Quest 3, but Unity account-owned packages must be imported from your Unity login.

## Current Local Finding

The installed Unity editor at:

```text
/Applications/Unity/Hub/Editor/6000.4.10f1
```

currently only has:

```text
WebGLSupport
```

Quest 3 builds require Android support. Install these modules from Unity Hub:

- Android Build Support
- Android SDK & NDK Tools
- OpenJDK

## Install Android Build Support

1. Open **Unity Hub**.
2. Go to **Installs**.
3. Find `6000.4.10f1`.
4. Click the gear or three-dot menu.
5. Choose **Add modules**.
6. Select:
   - **Android Build Support**
   - **Android SDK & NDK Tools**
   - **OpenJDK**
7. Install.

After install, this folder should exist:

```text
/Applications/Unity/Hub/Editor/6000.4.10f1/PlaybackEngines/AndroidPlayer
```

## Meta Packages

The project already includes Unity's official OpenXR stack:

- `com.unity.xr.openxr`
- `com.unity.xr.meta-openxr`
- `com.unity.xr.management`
- `com.unity.xr.interaction.toolkit`

Meta's own SDKs are distributed through Unity Package Manager / Asset Store and require your Unity account to add them to My Assets first. The current Meta XR SDK listing includes:

- Meta XR All-in-One SDK
- Meta XR Core SDK
- Meta XR Interaction SDK
- Meta Voice SDK
- Meta XR Audio SDK
- Meta MR Utility Kit

Import path:

1. In Unity, open **Window > Package Manager**.
2. Change the dropdown from **In Project** to **My Assets**.
3. Sign in if prompted.
4. Search **Meta XR SDK** or **Meta XR All-in-One SDK**.
5. Click **Download**.
6. Click **Import**.

If the package is not in My Assets, open the Asset Store listing in a browser while logged into your Unity account and click **Add to My Assets**.

## Purchased Assets

For your paid assets:

1. Open **Window > Package Manager**.
2. Select **My Assets**.
3. Sign into the Unity account that owns the purchases.
4. Search the asset name.
5. Download and Import.

Good first assets to import for this demo:

- avatar model,
- room/interior environment pack,
- UI/world-space panel pack,
- voice interaction or lipsync tools if available.

After importing an avatar model, replace the placeholder avatar in:

```text
Assets/Scenes/Quest3AvatarRoom.unity
```

Keep the `AvatarMoodController` on the avatar root and assign the model renderers to `avatarRenderers`.

## In-Unity Checklist

Open:

```text
VR Avatar Demo > Quest 3 Setup Checklist
```

This creates:

```text
Assets/Quest3SetupReport.txt
```

and reports whether Android Build Support and core XR packages are present.

## Quest 3 Build Checklist

1. Enable Developer Mode in the Meta Horizon mobile app.
2. Connect Quest 3 by USB-C.
3. Accept USB debugging inside the headset.
4. In Unity, open **File > Build Profiles**.
5. Switch target to **Android**.
6. Open **Project Settings > XR Plug-in Management**.
7. Enable **OpenXR** for Android.
8. In OpenXR settings, enable the **Meta Quest** feature group.
9. Open `Assets/Scenes/Quest3AvatarRoom.unity`.
10. Choose **Build And Run**.

