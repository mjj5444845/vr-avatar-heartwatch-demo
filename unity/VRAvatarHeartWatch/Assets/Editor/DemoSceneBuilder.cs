using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class DemoSceneBuilder
{
    [MenuItem("VR Avatar Demo/Build Quest 3 Demo Scene")]
    public static void BuildScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject root = new GameObject("VR Avatar Heart Watch Demo");
        HeartRateReceiver receiver = CreateReceiver(root.transform);
        LlmConversationController conversation = CreateConversation(root.transform);
        CreateRoom();
        Renderer[] avatarRenderers = CreateAvatar();
        CreateQuestRig();
        CreateWorldPanels(receiver, conversation);
        CreateLighting();

        AvatarMoodController mood = root.AddComponent<AvatarMoodController>();
        mood.receiver = receiver;
        mood.avatarRenderers = avatarRenderers;

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Quest3AvatarRoom.unity");
        EditorUtility.DisplayDialog("VR Avatar Demo", "Quest3AvatarRoom.unity has been generated.", "OK");
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

    private static void CreateRoom()
    {
        CreatePrimitive("Floor", PrimitiveType.Cube, new Vector3(0, -0.05f, 0), new Vector3(8, 0.1f, 8), new Color(0.70f, 0.74f, 0.80f));
        CreatePrimitive("Back Wall", PrimitiveType.Cube, new Vector3(0, 2, 4), new Vector3(8, 4, 0.12f), new Color(0.82f, 0.86f, 0.92f));
        CreatePrimitive("Left Wall", PrimitiveType.Cube, new Vector3(-4, 2, 0), new Vector3(0.12f, 4, 8), new Color(0.78f, 0.83f, 0.90f));
        CreatePrimitive("Right Wall", PrimitiveType.Cube, new Vector3(4, 2, 0), new Vector3(0.12f, 4, 8), new Color(0.78f, 0.83f, 0.90f));
        CreatePrimitive("Ceiling", PrimitiveType.Cube, new Vector3(0, 4, 0), new Vector3(8, 0.1f, 8), new Color(0.88f, 0.91f, 0.96f));
    }

    private static Renderer[] CreateAvatar()
    {
        GameObject body = CreatePrimitive("Avatar Body", PrimitiveType.Capsule, new Vector3(0, 1.05f, 2.25f), new Vector3(0.75f, 1.1f, 0.75f), new Color(0.16f, 0.25f, 0.38f));
        GameObject head = CreatePrimitive("Avatar Head", PrimitiveType.Sphere, new Vector3(0, 2.0f, 2.25f), new Vector3(0.62f, 0.62f, 0.62f), new Color(0.36f, 0.49f, 1.0f));
        GameObject aura = CreatePrimitive("Avatar Heart Aura", PrimitiveType.Cylinder, new Vector3(0, 1.55f, 2.18f), new Vector3(1.2f, 0.025f, 1.2f), new Color(0.15f, 0.72f, 0.50f));
        aura.transform.rotation = Quaternion.Euler(90, 0, 0);
        return new[] { body.GetComponent<Renderer>(), head.GetComponent<Renderer>(), aura.GetComponent<Renderer>() };
    }

    private static void CreateQuestRig()
    {
        GameObject rig = new GameObject("XR Origin - Quest 3");
        rig.transform.position = new Vector3(0, 0, -1.25f);

        GameObject cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.SetParent(rig.transform);
        cameraOffset.transform.localPosition = Vector3.zero;

        GameObject camera = new GameObject("Main Camera");
        camera.tag = "MainCamera";
        camera.transform.SetParent(cameraOffset.transform);
        camera.transform.localPosition = new Vector3(0, 1.6f, 0);
        camera.transform.localRotation = Quaternion.identity;
        camera.AddComponent<Camera>();
        camera.AddComponent<AudioListener>();

        CreateControllerPlaceholder("Left Controller", cameraOffset.transform, new Vector3(-0.32f, 1.2f, 0.55f), new Color(0.15f, 0.32f, 0.95f));
        CreateControllerPlaceholder("Right Controller", cameraOffset.transform, new Vector3(0.32f, 1.2f, 0.55f), new Color(0.95f, 0.32f, 0.15f));

        TryAddComponent(rig, "Unity.XR.CoreUtils.XROrigin, Unity.XR.CoreUtils");
    }

    private static void CreateControllerPlaceholder(string name, Transform parent, Vector3 localPosition, Color color)
    {
        GameObject controller = CreatePrimitive(name, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.12f, 0.24f, 0.12f), color);
        controller.transform.SetParent(parent);
        controller.transform.localPosition = localPosition;
        controller.transform.localRotation = Quaternion.Euler(70, 0, 0);
    }

    private static void CreateWorldPanels(HeartRateReceiver receiver, LlmConversationController conversation)
    {
        Canvas heartCanvas = CreatePanelCanvas("Heart Rate Panel", new Vector3(-1.6f, 1.55f, 1.15f), new Vector2(2.0f, 1.0f));
        Text heartTitle = CreateText(heartCanvas.transform, "Heart Rate Title", "Current heart rate", new Vector2(0, 110), 28, TextAnchor.MiddleCenter);
        Text heartValue = CreateText(heartCanvas.transform, "Heart Rate Value", "-- bpm", new Vector2(0, 20), 44, TextAnchor.MiddleCenter);
        Text heartZone = CreateText(heartCanvas.transform, "Heart Rate Zone", "waiting for Apple Watch", new Vector2(0, -70), 24, TextAnchor.MiddleCenter);
        _ = heartTitle;

        HeartRateWorldPanel panel = heartCanvas.gameObject.AddComponent<HeartRateWorldPanel>();
        panel.receiver = receiver;
        panel.heartRateText = heartValue;
        panel.zoneText = heartZone;

        Canvas dialogueCanvas = CreatePanelCanvas("Avatar Reply Panel", new Vector3(1.6f, 1.55f, 1.15f), new Vector2(2.3f, 1.2f));
        Text replyTitle = CreateText(dialogueCanvas.transform, "Reply Title", "Avatar reply", new Vector2(0, 130), 28, TextAnchor.MiddleCenter);
        Text replyText = CreateText(dialogueCanvas.transform, "Reply Text", "Say something to the avatar.", new Vector2(0, -10), 26, TextAnchor.MiddleCenter);
        _ = replyTitle;

        AvatarDialoguePanel dialoguePanel = dialogueCanvas.gameObject.AddComponent<AvatarDialoguePanel>();
        dialoguePanel.replyText = replyText;
        conversation.dialoguePanel = dialoguePanel;
    }

    private static Canvas CreatePanelCanvas(string name, Vector3 position, Vector2 meters)
    {
        GameObject canvasObject = new GameObject(name);
        canvasObject.transform.position = position;
        canvasObject.transform.rotation = Quaternion.Euler(0, 180, 0);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 24;
        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600, 320);
        canvasObject.transform.localScale = new Vector3(meters.x / 600f, meters.y / 320f, 1);

        Image background = canvasObject.AddComponent<Image>();
        background.color = new Color(0.05f, 0.08f, 0.13f, 0.82f);
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
        rect.sizeDelta = new Vector2(520, 120);
        rect.anchoredPosition = anchoredPosition;
        return text;
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Key Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightObject.transform.rotation = Quaternion.Euler(48, -28, 0);

        RenderSettings.ambientLight = new Color(0.42f, 0.46f, 0.52f);
    }

    private static GameObject CreatePrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.name = name;
        primitive.transform.position = position;
        primitive.transform.localScale = scale;
        primitive.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + " Material", color);
        return primitive;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.name = name;
        material.color = color;
        return material;
    }

    private static void TryAddComponent(GameObject target, string qualifiedTypeName)
    {
        Type type = Type.GetType(qualifiedTypeName);
        if (type != null && target.GetComponent(type) == null)
        {
            target.AddComponent(type);
        }
    }
}
