using System.IO;
using UnityEditor;
using UnityEditor.Animations;
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
        ScriptedConversationController conversation = CreateConversation(root.transform, receiver);

        CreateDayFloor(root.transform);
        GameObject avatar = CreateRobotKyle(root.transform);
        AvatarMotionController motionController = ConfigureAvatarForScriptedDialogue(avatar);
        CreateQuestRigIfMissing(root.transform);
        CreateWorldPanels(receiver, conversation);
        conversation.motionController = motionController;
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
            EditorUtility.DisplayDialog("VR Avatar Demo", "Minimal Quest 3 living room scene has been generated.", "OK");
        }
    }

    private static void PreserveBuildingBlocks()
    {
        foreach (Transform transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (!transform.name.StartsWith("[BuildingBlock]"))
            {
                continue;
            }

            if (IsAiBuildingBlock(transform.name))
            {
                Object.DestroyImmediate(transform.gameObject);
            }
            else
            {
                transform.SetParent(null, true);
            }
        }
    }

    private static bool IsAiBuildingBlock(string objectName)
    {
        return objectName.Contains("Speech To Text") || objectName.Contains("Text To Speech");
    }

    private static void RemoveGeneratedDemoObjects()
    {
        string[] names =
        {
            RootName,
            "Day Floor Stage",
            "Heart Rate Panel",
            "Avatar Reply Panel",
            "Day Sun",
            "Day Fill Light",
            "Meta Building Blocks Anchor",
            "[BuildingBlock] Speech To Text",
            "[BuildingBlock] Text To Speech"
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

    private static ScriptedConversationController CreateConversation(Transform parent, HeartRateReceiver receiver)
    {
        GameObject conversationObject = new GameObject("ScriptedConversationController");
        conversationObject.transform.SetParent(parent);

        ScriptedConversationController conversation = conversationObject.AddComponent<ScriptedConversationController>();
        conversation.apiBaseUrl = "http://127.0.0.1:8787";
        conversation.heartRateReceiver = receiver;
        conversation.proactiveTopicIntervalSeconds = 10f;

        QuestScriptedInputController voice = conversationObject.AddComponent<QuestScriptedInputController>();
        voice.conversationController = conversation;
        return conversation;
    }

    private static void CreateDayFloor(Transform parent)
    {
        GameObject stage = new GameObject("Day Floor Stage");
        stage.transform.SetParent(parent);

        InstantiatePrefabOrPrimitive(
            "Assets/ithappy/Furniture_Realistic/Prefabs/floor/floor_001.prefab",
            "Floor", PrimitiveType.Cube, new Vector3(0, -0.04f, 0), Quaternion.identity, new Vector3(7.5f, 0.08f, 7.5f),
            new Color(0.82f, 0.84f, 0.80f), stage.transform);
    }

    private static GameObject CreateRobotKyle(Transform parent)
    {
        GameObject avatar = Place("Assets/UnityTechnologies/SpaceRobotKyle/Prefabs/RobotKyle.prefab", "Robot Kyle - Avatar", new Vector3(0, 0, 1.2f), Quaternion.Euler(0, 180, 0), new Vector3(1.1f, 1.1f, 1.1f), parent);
        if (avatar == null)
        {
            avatar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            avatar.name = "Robot Kyle Missing - Placeholder Avatar";
            avatar.transform.SetParent(parent);
            avatar.transform.position = new Vector3(0, 0.95f, 1.2f);
            avatar.transform.localScale = new Vector3(0.55f, 0.95f, 0.55f);
        }

        GameObject lookAt = new GameObject("Avatar Conversation Focus");
        lookAt.transform.SetParent(avatar.transform);
        lookAt.transform.localPosition = new Vector3(0, 1.45f, 0.2f);
        return avatar;
    }

    private static AvatarMotionController ConfigureAvatarForScriptedDialogue(GameObject avatar)
    {
        if (avatar == null)
        {
            return null;
        }

        RemoveAvatarMovementComponents(avatar);

        Animator animator = avatar.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            return null;
        }

        AnimationClip idleClip = LoadClip("Assets/UnityTechnologies/SpaceRobotKyle/Animations/Stand--Idle.anim.fbx");
        AnimationClip saluteClip = LoadClip("Assets/AvatarMotion/Salute.fbx");
        AnimationClip happyClip = LoadClip("Assets/AvatarMotion/Happy Idle.fbx");
        AnimationClip defeatedClip = LoadClip("Assets/AvatarMotion/Defeated.fbx");
        AnimatorController controller = CreateScriptedAnimatorController(idleClip, saluteClip, happyClip, defeatedClip);
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }

        AvatarMotionController motionController = avatar.GetComponent<AvatarMotionController>();
        if (motionController == null)
        {
            motionController = avatar.AddComponent<AvatarMotionController>();
        }

        motionController.animator = animator;
        motionController.saluteClip = saluteClip;
        motionController.happyClip = happyClip;
        motionController.defeatedClip = defeatedClip;
        return motionController;
    }

    private static void RemoveAvatarMovementComponents(GameObject avatar)
    {
        foreach (CharacterController controller in avatar.GetComponentsInChildren<CharacterController>(true))
        {
            Object.DestroyImmediate(controller);
        }

        foreach (Rigidbody rigidbody in avatar.GetComponentsInChildren<Rigidbody>(true))
        {
            Object.DestroyImmediate(rigidbody);
        }

        foreach (MonoBehaviour behaviour in avatar.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour == null)
            {
                continue;
            }

            string typeName = behaviour.GetType().FullName;
            if (typeName == "StarterAssets.ThirdPersonController" ||
                typeName == "StarterAssets.StarterAssetsInputs" ||
                typeName == "StarterAssets.BasicRigidBodyPush" ||
                typeName == "UnityEngine.InputSystem.PlayerInput")
            {
                Object.DestroyImmediate(behaviour);
            }
        }
    }

    private static AnimatorController CreateScriptedAnimatorController(AnimationClip idleClip, AnimationClip saluteClip, AnimationClip happyClip, AnimationClip defeatedClip)
    {
        const string controllerPath = "Assets/AvatarMotion/ScriptedAvatarMotion.controller";
        Directory.CreateDirectory("Assets/AvatarMotion");

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            stateMachine.RemoveState(childState.state);
        }

        AnimatorState idleState = AddState(stateMachine, "Idle", idleClip, new Vector3(260, 80, 0));
        AddState(stateMachine, "Salute", saluteClip, new Vector3(260, 170, 0));
        AddState(stateMachine, "Happy", happyClip, new Vector3(260, 260, 0));
        AddState(stateMachine, "Defeated", defeatedClip, new Vector3(260, 350, 0));
        stateMachine.defaultState = idleState;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static AnimatorState AddState(AnimatorStateMachine stateMachine, string stateName, Motion motion, Vector3 position)
    {
        AnimatorState state = stateMachine.AddState(stateName, position);
        state.motion = motion;
        state.writeDefaultValues = true;
        return state;
    }

    private static AnimationClip LoadClip(string assetPath)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview"))
            {
                return clip;
            }
        }

        return AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
    }

    private static void CreateQuestRigIfMissing(Transform parent)
    {
        if (Object.FindAnyObjectByType<OVRCameraRig>() != null || GameObject.Find("Meta OVRCameraRig") != null)
        {
            return;
        }

        GameObject anchor = new GameObject("Meta Building Blocks Anchor");
        anchor.transform.SetParent(parent);
        anchor.transform.position = Vector3.zero;

        GameObject cameraRig = PlacePackagePrefab("Packages/com.meta.xr.sdk.core/Prefabs/OVRCameraRig.prefab", "Meta OVRCameraRig", new Vector3(0, 0, -1.6f), Quaternion.identity, Vector3.one, anchor.transform);
        if (cameraRig == null)
        {
            CreateQuestRigFallback(anchor.transform);
        }
    }

    private static void CreateQuestRigFallback(Transform parent)
    {
        GameObject rig = new GameObject("Fallback XR Camera Rig");
        rig.transform.SetParent(parent);
        rig.transform.position = new Vector3(0, 0, -1.6f);

        GameObject camera = new GameObject("Main Camera");
        camera.tag = "MainCamera";
        camera.transform.SetParent(rig.transform);
        camera.transform.localPosition = new Vector3(0, 1.6f, 0);
        camera.AddComponent<Camera>();
        camera.AddComponent<AudioListener>();
    }

    private static void CreateWorldPanels(HeartRateReceiver receiver, ScriptedConversationController conversation)
    {
        Canvas heartCanvas = CreatePanelCanvas("Heart Rate Panel", new Vector3(-1.35f, 1.35f, 0.35f), Quaternion.Euler(0, 20, 0), new Vector2(1.25f, 0.55f));
        Text heartValue = CreateText(heartCanvas.transform, "Heart Rate Value", "-- bpm", new Vector2(0, 36), 42, TextAnchor.MiddleCenter);
        Text heartZone = CreateText(heartCanvas.transform, "Heart Rate Zone", "waiting for Apple Watch", new Vector2(0, -52), 21, TextAnchor.MiddleCenter);

        HeartRateWorldPanel panel = heartCanvas.gameObject.AddComponent<HeartRateWorldPanel>();
        panel.receiver = receiver;
        panel.heartRateText = heartValue;
        panel.zoneText = heartZone;

        Canvas dialogueCanvas = CreatePanelCanvas("Avatar Reply Panel", new Vector3(1.2f, 1.65f, 0.3f), Quaternion.Euler(0, -20, 0), new Vector2(2.0f, 0.75f));
        Text replyText = CreateText(dialogueCanvas.transform, "Reply Text", "Press X to start, Y to switch, N/Enter or A for next.", new Vector2(0, 0), 25, TextAnchor.MiddleCenter);

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
        GameObject sunObject = new GameObject("Day Sun");
        Light sun = sunObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.25f;
        sun.color = new Color(1.0f, 0.96f, 0.88f);
        sunObject.transform.rotation = Quaternion.Euler(48, -25, 0);

        GameObject fillObject = new GameObject("Day Fill Light");
        Light fill = fillObject.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.intensity = 1.0f;
        fill.range = 6f;
        fill.color = new Color(0.72f, 0.82f, 1.0f);
        fillObject.transform.position = new Vector3(0, 2.8f, -1.4f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.72f, 0.82f, 1.0f);
        RenderSettings.ambientEquatorColor = new Color(0.83f, 0.86f, 0.83f);
        RenderSettings.ambientGroundColor = new Color(0.62f, 0.58f, 0.52f);
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
