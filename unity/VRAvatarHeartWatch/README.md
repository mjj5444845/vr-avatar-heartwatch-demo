# VRAvatarHeartWatch Unity Project

This is a lightweight Unity 6000.4.10f1 project shell for the Quest 3 VR scene.

## Unity Startup Note

This machine initially needed Rosetta 2 before Unity could start. If Unity shows this error again:

```text
Rosetta 2 isn't installed. The macOS Unity Editor requires Rosetta 2 to operate.
```

Install Rosetta:

```bash
softwareupdate --install-rosetta --agree-to-license
```

Then open this folder from Unity Hub.

## Generate The Scene

1. Open `unity/VRAvatarHeartWatch` in Unity Hub.
2. Let Unity restore packages.
3. Open the top menu:

```text
VR Avatar Demo > Build Quest 3 Demo Scene
```

4. Open or save the generated scene:

```text
Assets/Scenes/Quest3AvatarRoom.unity
```

The scene includes:

- a simple room,
- a placeholder avatar,
- a Quest 3 XR Origin/camera/controller object structure,
- a world-space current heart-rate panel,
- a world-space avatar reply panel,
- scripts for polling SQLite API heart rate,
- scripts for submitting speech transcripts to the LLM bridge.

## Quest 3 Controls

The package manifest includes:

- XR Interaction Toolkit,
- XR Management,
- OpenXR,
- Unity OpenXR Meta,
- Input System.

Run this project menu item after Unity finishes importing packages:

```text
VR Avatar Demo > Quest 3 Setup Checklist
```

After packages resolve, enable Android OpenXR in:

```text
Edit > Project Settings > XR Plug-in Management
```

For full Meta controller/hand support, import Meta XR SDK / Meta XR All-in-One SDK from Unity Package Manager or the Meta developer download, then replace or extend the generated controller placeholders with Meta's rig/prefabs.

For account-owned assets and Meta SDK import steps, see:

```text
../../docs/unity-assets-and-meta-sdk.md
```

## Voice + LLM

`QuestVoiceInputController` exposes:

```csharp
SubmitMetaVoiceTranscript(string transcript)
```

Connect Meta Voice SDK Dictation/STT final transcript events to that method. The script sends the transcript to:

```text
POST /api/chat
```

The API returns an avatar reply and the reply panel updates in VR.
