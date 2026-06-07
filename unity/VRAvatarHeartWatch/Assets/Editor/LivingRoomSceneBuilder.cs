using System.IO;
using Meta.XR.BuildingBlocks.AIBlocks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LivingRoomSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Quest3LivingRoomAvatar.unity";
    private const string RootName = "Quest 3 Living Room Avatar Demo";

    [MenuItem("VR Avatar Demo/Build Living Room Avatar Scene")]
    public static void BuildScene()
    {
        if (File.Exists(ScenePath))
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            PreserveBuildingBlocks();
            RemoveGeneratedDemoObjects();
        }
        else
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        GameObject root = new GameObject(RootName);
        HeartRateReceiver receiver = CreateReceiver(root.transform);
        LlmConversationController conversation = CreateConversation(root.transform, receiver);
        QuestVoiceInputController voice = conversation.GetComponent<QuestVoiceInputController>();

        CreateSimpleRoom(root.transform);
        CreateSofaAndTv(root.transform);
        GameObject avatar = CreateRobotKyle(root.transform);
        CreateQuestRigIfMissing(root.transform);
        CreateWorldPanels(receiver, conversation);
        CreateLighting();
        WireMetaBuildingBlocks(conversation, voice);

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
            EditorUtility.DisplayDialog("VR Avatar Demo", "Minimal Quest 3 living room scene has been generated.", "OK");
        }
    }

    private static void PreserveBuildingBlocks()
    {
        foreach (Transform transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (transform.name.StartsWith("[BuildingBlock]"))
            {
                transform.SetParent(null, true);
            }
        }
    }

    private static void RemoveGeneratedDemoObjects()
    {
        string[] names =
        {
            RootName,
            "Living Room Shell",
            "Furniture Realistic Layout",
            "Heart Rate Panel",
            "Avatar Reply Panel",
            "Soft Window Key Light",
            "Warm Living Room Lamp Glow",
            "Meta Building Blocks Anchor"
        };

        foreach (string name in names)
        {
            GameObject gameObject = GameObject.Find(name);
            if (gameObject != null)
            {
                Object.DestroyImmediate(gameObject);
            }
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

    private static LlmConversationController CreateConversation(Transform parent, HeartRateReceiver receiver)
    {
        GameObject conversationObject = new GameObject("LLMConversationController");
        conversationObject.transform.SetParent(parent);

        LlmConversationController conversation = conversationObject.AddComponent<LlmConversationController>();
        conversation.apiBaseUrl = "http://127.0.0.1:8787";
        conversation.heartRateReceiver = receiver;
        conversation.proactiveTopicIntervalSeconds = 10f;

        QuestVoiceInputController voice = conversationObject.AddComponent<QuestVoiceInputController>();
        voice.conversationController = conversation;
        return conversation;
    }

    private static void CreateSimpleRoom(Transform parent)
    {
        GameObject room = new GameObject("Living Room Shell");
        room.transform.SetParent(parent);

        InstantiatePrefabOrPrimitive(
            "Assets/ithappy/Furniture_Realistic/Prefabs/floor/floor_001.prefab",
            "Floor", PrimitiveType.Cube, new Vector3(0, -0.04f, 0), Quaternion.identity, new Vector3(5.5f, 0.08f, 4.4f),
            new Color(0.56f, 0.46f, 0.34f), room.transform);
        InstantiatePrefabOrPrimitive(
            "Assets/ithappy/Furniture_Realistic/Prefabs/wall/wall_001.prefab",
            "Back Wall", PrimitiveType.Cube, new Vector3(0, 1.55f, 2.25f), Quaternion.identity, new Vector3(5.5f, 3.1f, 0.12f),
            new Color(0.78f, 0.80f, 0.77f), room.transform);
        InstantiatePrefabOrPrimitive(
            "Assets/ithappy/Furniture_Realistic/Prefabs/wall/wall_002.prefab",
            "Left Wall", PrimitiveType.Cube, new Vector3(-2.75f, 1.55f, 0), Quaternion.identity, new Vector3(0.12f, 3.1f, 4.4f),
            new Color(0.72f, 0.75f, 0.73f), room.transform);
        InstantiatePrefabOrPrimitive(
            "Assets/ithappy/Furniture_Realistic/Prefabs/wall/wall_003.prefab",
            "Right Wall", PrimitiveType.Cube, new Vector3(2.75f, 1.55f, 0), Quaternion.identity, new Vector3(0.12f, 3.1f, 4.4f),
            new Color(0.72f, 0.75f, 0.73f), room.transform);
    }

    private static void CreateSofaAndTv(Transform parent)
    {
        GameObject furniture = new GameObject("Furniture Realistic Layout");
        furniture.transform.SetParent(parent);

        Place("Assets/ithappy/Furniture_Realistic/Prefabs/sofa/sofa_003.prefab", "Sofa", new Vector3(0, 0, 1.25f), Quaternion.Euler(0, 180, 0), new Vector3(1.2f, 1.2f, 1.2f), furniture.transform);
        Place("Assets/ithappy/Furniture_Realistic/Prefabs/electronics/electronics_001.prefab", "Television", new Vector3(0, 0.9f, -1.9f), Quaternion.identity, new Vector3(1.1f, 1.1f, 1.1f), furniture.transform);
    }

    private static GameObject CreateRobotKyle(Transform parent)
    {
        GameObject avatar = Place("Assets/UnityTechnologies/SpaceRobotKyle/Prefabs/RobotKyle.prefab", "Robot Kyle - Avatar", new Vector3(1.35f, 0, -0.15f), Quaternion.Euler(0, -120, 0), new Vector3(1.1f, 1.1f, 1.1f), parent);
        if (avatar == null)
        {
            avatar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            avatar.name = "Robot Kyle Missing - Placeholder Avatar";
            avatar.transform.SetParent(parent);
            avatar.transform.position = new Vector3(1.35f, 0.95f, -0.15f);
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

    private static void CreateQuestRigIfMissing(Transform parent)
    {
        if (Object.FindFirstObjectByType<OVRCameraRig>() != null || GameObject.Find("Meta OVRCameraRig") != null)
        {
            return;
        }

        GameObject anchor = new GameObject("Meta Building Blocks Anchor");
        anchor.transform.SetParent(parent);
        anchor.transform.position = Vector3.zero;

        GameObject cameraRig = PlacePackagePrefab("Packages/com.meta.xr.sdk.core/Prefabs/OVRCameraRig.prefab", "Meta OVRCameraRig", new Vector3(0, 0, -1.2f), Quaternion.identity, Vector3.one, anchor.transform);
        if (cameraRig == null)
        {
            CreateQuestRigFallback(anchor.transform);
        }
    }

    private static void CreateQuestRigFallback(Transform parent)
    {
        GameObject rig = new GameObject("Fallback XR Camera Rig");
        rig.transform.SetParent(parent);
        rig.transform.position = new Vector3(0, 0, -1.2f);

        GameObject camera = new GameObject("Main Camera");
        camera.tag = "MainCamera";
        camera.transform.SetParent(rig.transform);
        camera.transform.localPosition = new Vector3(0, 1.6f, 0);
        camera.AddComponent<Camera>();
        camera.AddComponent<AudioListener>();
    }

    private static void CreateWorldPanels(HeartRateReceiver receiver, LlmConversationController conversation)
    {
        Canvas heartCanvas = CreatePanelCanvas("Heart Rate Panel", new Vector3(-1.45f, 1.35f, -0.8f), Quaternion.Euler(0, 25, 0), new Vector2(1.25f, 0.55f));
        Text heartValue = CreateText(heartCanvas.transform, "Heart Rate Value", "-- bpm", new Vector2(0, 36), 42, TextAnchor.MiddleCenter);
        Text heartZone = CreateText(heartCanvas.transform, "Heart Rate Zone", "waiting for Apple Watch", new Vector2(0, -52), 21, TextAnchor.MiddleCenter);

        HeartRateWorldPanel panel = heartCanvas.gameObject.AddComponent<HeartRateWorldPanel>();
        panel.receiver = receiver;
        panel.heartRateText = heartValue;
        panel.zoneText = heartZone;

        Canvas dialogueCanvas = CreatePanelCanvas("Avatar Reply Panel", new Vector3(0.95f, 1.7f, -0.85f), Quaternion.Euler(0, -18, 0), new Vector2(2.0f, 0.75f));
        Text replyText = CreateText(dialogueCanvas.transform, "Reply Text", "Press A and talk. I will reply here.", new Vector2(0, 0), 25, TextAnchor.MiddleCenter);

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
        sun.intensity = 1.0f;
        sunObject.transform.rotation = Quaternion.Euler(42, -32, 0);

        GameObject warmLampObject = new GameObject("Warm Living Room Lamp Glow");
        Light warmLamp = warmLampObject.AddComponent<Light>();
        warmLamp.type = LightType.Point;
        warmLamp.intensity = 1.8f;
        warmLamp.range = 4f;
        warmLamp.color = new Color(1.0f, 0.78f, 0.52f);
        warmLampObject.transform.position = new Vector3(1.7f, 1.8f, 1.2f);

        RenderSettings.ambientLight = new Color(0.36f, 0.38f, 0.42f);
    }

    private static void WireMetaBuildingBlocks(LlmConversationController conversation, QuestVoiceInputController voice)
    {
        SpeechToTextAgent stt = Object.FindFirstObjectByType<SpeechToTextAgent>();
        TextToSpeechAgent tts = Object.FindFirstObjectByType<TextToSpeechAgent>();
        AIProviderBase sttProvider = AssetDatabase.LoadAssetAtPath<AIProviderBase>("Assets/MetaXR/SpeechToText_OpenAI_ProviderProfile.asset");
        AIProviderBase ttsProvider = AssetDatabase.LoadAssetAtPath<AIProviderBase>("Assets/MetaXR/TextToSpeech_OpenAI_ProviderProfile.asset");

        AssignProvider(stt, sttProvider);
        AssignProvider(tts, ttsProvider);

        if (voice != null)
        {
            voice.speechToTextAgent = stt;
        }

        if (conversation != null)
        {
            conversation.textToSpeechAgent = tts;
        }
    }

    private static void AssignProvider(Object agent, AIProviderBase provider)
    {
        if (agent == null || provider == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(agent);
        SerializedProperty providerProperty = serializedObject.FindProperty("providerAsset");
        if (providerProperty != null)
        {
            providerProperty.objectReferenceValue = provider;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(agent);
        }
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
