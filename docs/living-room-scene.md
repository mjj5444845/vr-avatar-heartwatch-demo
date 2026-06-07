# Day Floor Avatar Scene

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

- a daylight floor stage,
- Robot Kyle standing as the conversation avatar,
- current heart-rate panel,
- avatar reply panel.
- three scripted avatar gesture clips from `Assets/AvatarMotion`.

The Quest view starts in front of Robot Kyle, facing the avatar. If no Meta camera rig exists in the scene, the builder adds `OVRCameraRig`. Existing Meta Building Blocks are preserved.

## Scripted Dialogue Flow

- Press the Quest 3 left-hand `X` button to start the selected scripted scene.
- Press the Quest 3 left-hand `Y` button to switch between the three prepared dialogue scenes.
- Press the Quest 3 right-hand `A` button to advance to the next line.
- In the Unity Editor, use `X`, `Y`, and `N` or Enter as fallback test keys.

This version does not call AI, speech-to-text, or text-to-speech. `ScriptedConversationController` displays prepared user/avatar lines and writes each line to the API through `/api/chat/records`.

Robot Kyle's Starter Assets movement inputs are removed from the generated scene, so keyboard `WASD` and Space do not move or jump the avatar.

All scripted dialogue text is also mirrored in the repository root file:

```text
scripted-dialogues.json
```

When the user has not advanced the dialogue for 10 seconds, `ScriptedConversationController` reads the latest heart-rate sample from `HeartRateReceiver` and picks a prepared avatar-initiated prompt. Each heart-rate zone has at least five possible pseudo replies.

Conversation records are stored by the API with:

- `role`: `user` or `avatar`
- `messageType`: `user_speech`, `avatar_reply`, `sensor_prompt`, or `system`
- `conversationInitiator`: `user` or `avatar`
- `heartRate` and `zone` when a sample is available

The iPhone app reads these rows from `/api/chat`.

## Meta Building Blocks

The scene preserves Meta Building Blocks that you add through Meta's Building Blocks window, but the scripted flow does not depend on AI blocks.

```text
Any existing [BuildingBlock] objects
```
