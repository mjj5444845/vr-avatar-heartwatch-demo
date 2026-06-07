using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class LivingRoomSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Quest3LivingRoomAvatar.unity";

    [MenuItem("VR Avatar Demo/Build Living Room Avatar Scene")]
    public static void BuildScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject root = new GameObject("Quest 3 Living Room Avatar Demo");
        HeartRateReceiver receiver = CreateReceiver(root.transform);
        LlmConversationController conversation = CreateConversation(root.transform);

        CreateLivingRoomShell(root.transform);
        CreateFurnitureLayout(root.transform);
        GameObject avatar = CreateRobotKyle(root.transform);
        CreateMetaBuildingBlockRig(root.transform);
        CreateWorldPanels(receiver, conversation);
        CreateLighting();

        if (avatar != null)
        {
            AvatarMoodController mood = root.AddComponent<AvatarMoodController>();
            mood.receiver = receiver;
            mood.avatarRenderers = avatar.GetComponentsInChildren<Renderer>();
        }

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("VR Avatar Demo", "Quest3LivingRoomAvatar.unity has been generated.", "OK");
        }
    }

    private static HeartRateReceiver CreateReceiver(Transform parent)
    {
        GameObject receiverObject = new GameObject("HeartRateReceiver");
        receiverObject.transform.SetParent(parent);
        HeartRateReceiver receiver = receiverObject.AddComponent<HeartRateReceiver>();
        receiver.apiBaseUrl = "http://127.0.0.1:8787";
        return receiver;
    }

    private static LlmConversationController CreateConversation(Transform parent)
    {
        GameObject conversationObject = new GameObject("LLMConversationController");
        conversationObject.transform.SetParent(parent);
        LlmConversationController conversation = conversationObject.AddComponent<LlmConversationController>();
        conversation.apiBaseUrl = "http://127.0.0.1:8787";
        conversationObject.AddComponent<QuestVoiceInputController>().conversationController = conversation;
        return conversation;
    }

    private static void CreateLivingRoomShell(Transform parent)
    {
        GameObject room = new GameObject("Living Room Shell");
        room.transform.SetParent(parent);

        InstantiatePrefabOrPrimitive(
            "Assets/ithappy/Furniture_Realistic/Prefabs/floor/floor_001.prefab",
            "Floor", PrimitiveType.Cube, new Vector3(0, -0.04f, 0), Quaternion.identity, new Vector3(6.6f, 0.08f, 5.2f),
            new Color(0.56f, 0.46f, 0.34f), room.transform);
        InstantiatePrefabOrPrimitive(
            "Assets/ithappy/Furniture_Realistic/Prefabs/wall/wall_001.prefab",
            "Back Wall", PrimitiveType.Cube, new Vector3(0, 1.55f, 2.65f), Quaternion.identity, new Vector3(6.6f, 3.1f, 0.12f),
            new Color(0.78f, 0.80f, 0.77f), room.transform);
        InstantiatePrefabOrPrimitive(
            "Assets/ithappy/Furniture_Realistic/Prefabs/wall/wall_002.prefab",
            "Left Wall", PrimitiveType.Cube, new Vector3(-3.3f, 1.55f, 0), Quaternion.identity, new Vector3(0.12f, 3.1f, 5.2f),
            new Color(0.72f, 0.75f, 0.73f), room.transform);
        InstantiatePrefabOrPrimitive(
            "Assets/ithappy/Furniture_Realistic/Prefabs/wall/wall_003.prefab",
            "Right Wall", PrimitiveType.Cube, new Vector3(3.3f, 1.55f, 0), Quaternion.identity, new Vector3(0.12f, 3.1f, 5.2f),
            new Color(0.72f, 0.75f, 0.73f), room.transform);
        InstantiatePrefabOrPrimitive(
            "Assets/ithappy/Furniture_Realistic/Prefabs/window/window_001.prefab",
            "Window", PrimitiveType.Cube, new Vector3(-1.9f, 1.55f, 2.55f), Quaternion.Euler(0, 180, 0), new Vector3(1.15f, 0.85f, 0.08f),
            new Color(0.55f, 0.75f, 0.92f), room.transform);
        InstantiatePrefabOrPrimitive(
            "Assets/ithappy/Furniture_Realistic/Prefabs/curtain/curtain_001.prefab",
            "Curtains", PrimitiveType.Cube, new Vector3(-1.9f, 1.55f, 2.48f), Quaternion.Euler(0, 180, 0), new Vector3(1.65f, 1.2f, 0.05f),
            new Color(0.28f, 0.34f, 0.45f), room.transform);
    }

    private static void CreateFurnitureLayout(Transform parent)
    {
        GameObject furniture = new GameObject("Furniture Realistic Layout");
        furniture.transform.SetParent(parent);

        Place("Assets/ithappy/Furniture_Realistic/Prefabs/sofa/sofa_003.prefab", "Sofa", new Vector3(0, 0, 1.75f), Quaternion.Euler(0, 180, 0), new Vector3(1.2f, 1.2f, 1.2f), furniture.transform);
        Place("Assets/ithappy/Furniture_Realistic/Prefabs/coffee_table/coffee_table_006.prefab", "Coffee Table", new Vector3(0, 0, 0.45f), Quaternion.identity, Vector3.one, furniture.transform);
        Place("Assets/ithappy/Furniture_Realistic/Prefabs/carpet/carpet_004.prefab", "Area Rug", new Vector3(0, 0.01f, 0.35f), Quaternion.identity, new Vector3(1.3f, 1.3f, 1.3f), furniture.transform);
        Place("Assets/ithappy/Furniture_Realistic/Prefabs/entertainment/entertainment_004.prefab", "TV Console", new Vector3(0, 0, -2.15f), Quaternion.identity, Vector3.one, furniture.transform);
        Place("Assets/ithappy/Furniture_Realistic/Prefabs/electronics/electronics_001.prefab", "Television", new Vector3(0, 0.92f, -2.23f), Quaternion.identity, new Vector3(1.05f, 1.05f, 1.05f), furniture.transform);
        Place("Assets/ithappy/Furniture_Realistic/Prefabs/lamp/lamp_006.prefab", "Floor Lamp", new Vector3(2.35f, 0, 1.35f), Quaternion.Euler(0, -25, 0), Vector3.one, furniture.transform);
        Place("Assets/ithappy/Furniture_Realistic/Prefabs/shelf/shelf_004.prefab", "Side Shelf", new Vector3(-2.55f, 0, -1.15f), Quaternion.Euler(0, 90, 0), Vector3.one, furniture.transform);
        Place("Assets/ithappy/Furniture_Realistic/Prefabs/flower/flower_003.prefab", "Plant", new Vector3(-2.45f, 0, 1.55f), Quaternion.identity, Vector3.one, furniture.transform);
        Place("Assets/ithappy/Furniture_Realistic/Prefabs/picture/picture_006.prefab", "Wall Art", new Vector3(1.55f, 1.75f, 2.49f), Quaternion.Euler(0, 180, 0), Vector3.one, furniture.transform);
    }

    private static GameObject CreateRobotKyle(Transform parent)
    {
        GameObject avatar = Place("Assets/UnityTechnologies/SpaceRobotKyle/Prefabs/RobotKyle.prefab", "Robot Kyle - Avatar", new Vector3(1.45f, 0, 0.1f), Quaternion.Euler(0, -125, 0), new Vector3(1.1f, 1.1f, 1.1f), parent);
        if (avatar == null)
        {
            avatar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            avatar.name = "Robot Kyle Missing - Placeholder Avatar";
            avatar.transform.SetParent(parent);
            avatar.transform.position = new Vector3(1.45f, 0.95f, 0.1f);
            avatar.transform.localScale = new Vector3(0.55f, 0.95f, 0.55f);
        }

        Animator animator = avatar.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            RuntimeAnimatorController idleController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/UnityTechnologies/SpaceRobotKyle/Animations/StarterAssetsThirdPerson.controller");
            if (idleController != null)
            {
                animator.runtimeAnimatorController = idleController;
            }
        }

        GameObject lookAt = new GameObject("Avatar Conversation Focus");
        lookAt.transform.SetParent(avatar.transform);
        lookAt.transform.localPosition = new Vector3(0, 1.45f, 0.2f);
        return avatar;
    }

    private static void CreateMetaBuildingBlockRig(Transform parent)
    {
        GameObject anchor = new GameObject("Meta Building Blocks Anchor");
        anchor.transform.SetParent(parent);
        anchor.transform.position = Vector3.zero;

        GameObject cameraRig = PlacePackagePrefab("Packages/com.meta.xr.sdk.core/Prefabs/OVRCameraRig.prefab", "Meta OVRCameraRig", new Vector3(0, 0, -1.25f), Quaternion.identity, Vector3.one, anchor.transform);
        if (cameraRig == null)
        {
            CreateQuestRigFallback(anchor.transform);
        }

        PlacePackagePrefab("Packages/com.meta.xr.sdk.interaction.ovr/Runtime/Prefabs/OVRInteractionComprehensive.prefab", "Meta OVR Interaction Comprehensive", Vector3.zero, Quaternion.identity, Vector3.one, anchor.transform);
        PlacePackagePrefab("Packages/com.meta.xr.sdk.interaction.ovr/Runtime/Prefabs/OVRControllerDrivenHands.prefab", "Meta Controller Driven Hands", Vector3.zero, Quaternion.identity, Vector3.one, anchor.transform);
    }

    private static void CreateQuestRigFallback(Transform parent)
    {
        GameObject rig = new GameObject("Fallback XR Camera Rig");
        rig.transform.SetParent(parent);
        rig.transform.position = new Vector3(0, 0, -1.25f);

        GameObject camera = new GameObject("Main Camera");
        camera.tag = "MainCamera";
        camera.transform.SetParent(rig.transform);
        camera.transform.localPosition = new Vector3(0, 1.6f, 0);
        camera.AddComponent<Camera>();
        camera.AddComponent<AudioListener>();
    }

    private static void CreateWorldPanels(HeartRateReceiver receiver, LlmConversationController conversation)
    {
        Canvas heartCanvas = CreatePanelCanvas("Heart Rate Panel", new Vector3(-1.75f, 1.45f, -0.95f), Quaternion.Euler(0, 25, 0), new Vector2(1.45f, 0.75f));
        Text heartValue = CreateText(heartCanvas.transform, "Heart Rate Value", "-- bpm", new Vector2(0, 28), 42, TextAnchor.MiddleCenter);
        Text heartZone = CreateText(heartCanvas.transform, "Heart Rate Zone", "waiting for Apple Watch", new Vector2(0, -55), 22, TextAnchor.MiddleCenter);

        HeartRateWorldPanel panel = heartCanvas.gameObject.AddComponent<HeartRateWorldPanel>();
        panel.receiver = receiver;
        panel.heartRateText = heartValue;
        panel.zoneText = heartZone;

        Canvas dialogueCanvas = CreatePanelCanvas("Avatar Reply Panel", new Vector3(1.0f, 1.75f, -0.72f), Quaternion.Euler(0, -18, 0), new Vector2(2.2f, 0.9f));
        Text replyText = CreateText(dialogueCanvas.transform, "Reply Text", "Hi, I am Robot Kyle. Ask me how your session feels.", new Vector2(0, 0), 25, TextAnchor.MiddleCenter);

        AvatarDialoguePanel dialoguePanel = dialogueCanvas.gameObject.AddComponent<AvatarDialoguePanel>();
        dialoguePanel.replyText = replyText;
        conversation.dialoguePanel = dialoguePanel;
    }

    private static Canvas CreatePanelCanvas(string name, Vector3 position, Quaternion rotation, Vector2 meters)
    {
        GameObject canvasObject = new GameObject(name);
        canvasObject.transform.position = position;
        canvasObject.transform.rotation = rotation;

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 24;
        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(620, 280);
        canvasObject.transform.localScale = new Vector3(meters.x / 620f, meters.y / 280f, 1);

        Image background = canvasObject.AddComponent<Image>();
        background.color = new Color(0.04f, 0.06f, 0.09f, 0.82f);
        return canvas;
    }

    private static Text CreateText(Transform parent, string name, string value, Vector2 anchoredPosition, int fontSize, TextAnchor anchor)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.alignment = anchor;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(550, 155);
        rect.anchoredPosition = anchoredPosition;
        return text;
    }

    private static void CreateLighting()
    {
        GameObject sunObject = new GameObject("Soft Window Key Light");
        Light sun = sunObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.1f;
        sunObject.transform.rotation = Quaternion.Euler(42, -32, 0);

        GameObject warmLampObject = new GameObject("Warm Living Room Lamp Glow");
        Light warmLamp = warmLampObject.AddComponent<Light>();
        warmLamp.type = LightType.Point;
        warmLamp.intensity = 2.4f;
        warmLamp.range = 5f;
        warmLamp.color = new Color(1.0f, 0.78f, 0.52f);
        warmLampObject.transform.position = new Vector3(2.35f, 1.6f, 1.35f);

        RenderSettings.ambientLight = new Color(0.36f, 0.38f, 0.42f);
    }

    private static GameObject Place(string path, string name, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent)
    {
        return InstantiatePrefab(path, name, position, rotation, scale, parent);
    }

    private static GameObject PlacePackagePrefab(string path, string name, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent)
    {
        return InstantiatePrefab(path, name, position, rotation, scale, parent);
    }

    private static void InstantiatePrefabOrPrimitive(string path, string name, PrimitiveType fallback, Vector3 position, Quaternion rotation, Vector3 scale, Color fallbackColor, Transform parent)
    {
        GameObject instance = InstantiatePrefab(path, name, position, rotation, scale, parent);
        if (instance != null)
        {
            return;
        }

        GameObject primitive = GameObject.CreatePrimitive(fallback);
        primitive.name = name;
        primitive.transform.SetParent(parent);
        primitive.transform.position = position;
        primitive.transform.rotation = rotation;
        primitive.transform.localScale = scale;
        primitive.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + " Material", fallbackColor);
    }

    private static GameObject InstantiatePrefab(string path, string name, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"Missing prefab: {path}");
            return null;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = name;
        instance.transform.SetParent(parent);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = scale;
        return instance;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.name = name;
        material.color = color;
        return material;
    }
}

