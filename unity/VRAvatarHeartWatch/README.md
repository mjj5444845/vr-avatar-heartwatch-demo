# VRAvatarHeartWatch Unity Project

This is the Quest 3 demo scene project for Unity `6000.4.10f1`.

Current main scene:

```text
Assets/Scenes/Quest3LivingRoomAvatar.unity
```

Scene contents:

- Daytime floor-only scene.
- Robot Kyle avatar.
- Quest 3 camera and controller input.
- Live heart-rate panel.
- Avatar dialogue panel.
- Start, exit, scripted dialogue, and SQLite write logic.

## Quest 3 Controls

- Right-hand **B**: start the demo and write `demo_start`.
- Right-hand **A**: next scripted dialogue line.
- Left-hand **X**: start or replay the current script.
- Left-hand **Y**: switch scripts.
- Right-hand **Menu**: exit the VR app, write `demo_stop`, and stop live recording.

Unity Editor keyboard:

- `B`: start the demo.
- `N` or `Enter`: next line.
- `X`: start or replay the current script.
- `Y`: switch script.
- `Q`: exit and stop recording.

## Scene Settings

- Camera rig height is set to `Y = 1.5`.
- Heart-rate and dialogue panels have no rotation so they face the viewer directly.
- Only the standard daylight directional light is enabled; extra warm/fill lights are disabled.
- Panel text is enlarged for Quest presentation readability.

## Build Outputs

Unity menu:

```text
VR Avatar Demo > Build > macOS Demo App
VR Avatar Demo > Build > Windows Demo App
VR Avatar Demo > Build > Quest 3 Android APK
```

Output paths:

```text
../../release/macOS/VRAvatarHeartWatch.app
../../release/Windows/VRAvatarHeartWatch.exe
../../release/Quest3/VRAvatarHeartWatch.apk
```

Windows builds require Unity Windows Build Support. Quest builds require Android Build Support and Quest/OpenXR setup.
