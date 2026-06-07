# Living Room Scene

The generated scene is:

```text
unity/VRAvatarHeartWatch/Assets/Scenes/Quest3LivingRoomAvatar.unity
```

It uses locally imported assets:

- `Assets/UnityTechnologies/SpaceRobotKyle/Prefabs/RobotKyle.prefab`
- `Assets/ithappy/Furniture_Realistic/Prefabs/...`

These full asset folders are intentionally ignored by Git because the repository is public and those assets may be licensed through your Unity account. The scene and builder script can be committed, but a fresh clone must import Robot Kyle and Furniture Realistic before regenerating or opening the scene correctly.

## Generate Again

In Unity:

```text
VR Avatar Demo > Build Living Room Avatar Scene
```

The scene is intentionally minimal. It includes:

- a compact living room shell,
- one sofa,
- one television/electronics prefab,
- Robot Kyle standing as the conversation avatar,
- current heart-rate panel,
- avatar reply panel.

If no Meta camera rig exists in the scene, the builder adds `OVRCameraRig`. Existing Meta Building Blocks are preserved.

## Voice Flow

- Press the Quest 3 right-hand `A` button to start listening.
- The scene uses Meta's `[BuildingBlock] Speech To Text` agent when it exists.
- The transcript is sent to the local API at `http://127.0.0.1:8787/api/chat`.
- Robot Kyle's reply is shown on the avatar reply panel.
- If `[BuildingBlock] Text To Speech` exists, the same reply is spoken aloud.
- In the Unity Editor, press Space as a fallback test trigger.

When the user has not spoken for 10 seconds, `LlmConversationController` reads the latest heart-rate sample from `HeartRateReceiver` and asks the avatar to start a short check-in topic.

## Meta Building Blocks

The scene preserves Meta Building Blocks that you add through Meta's Building Blocks window, especially:

```text
[BuildingBlock] Speech To Text
[BuildingBlock] Text To Speech
```

Two provider profiles live in `Assets/MetaXR`:

- `SpeechToText_OpenAI_ProviderProfile.asset` uses `gpt-4o-mini-transcribe`.
- `TextToSpeech_OpenAI_ProviderProfile.asset` uses `gpt-4o-mini-tts`.

Do not commit an API key. Set credentials locally through Meta's provider UI or Unity's credential storage.
