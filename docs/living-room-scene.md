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

The scene includes:

- a compact living room shell,
- sofa,
- coffee table,
- rug,
- TV console,
- television/electronics,
- lamp,
- shelf,
- plant,
- wall art,
- Robot Kyle standing as the conversation avatar,
- Meta `OVRCameraRig`,
- Meta `OVRInteractionComprehensive`,
- Meta `OVRControllerDrivenHands`,
- current heart-rate panel,
- avatar reply panel.

## Meta Building Blocks

The scene instantiates Meta prefabs from installed packages when available. You can also add or replace components through Meta's Building Blocks window in Unity. Use the object named:

```text
Meta Building Blocks Anchor
```

as the organization point for added Camera Rig, Controller/Hand, Interaction, Voice, and Passthrough blocks.

