using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ChatRecordRequest
{
    public string role;
    public string text;
    public string messageType;
    public string conversationInitiator;
    public int heartRate;
    public string zone;
}

[Serializable]
public class DemoEventRequest
{
    public string type;
    public string text;
    public string source;
}

public class ScriptedConversationController : MonoBehaviour
{
    public string apiBaseUrl = "http://127.0.0.1:8787";
    public AvatarDialoguePanel dialoguePanel;
    public HeartRateReceiver heartRateReceiver;
    public AvatarMotionController motionController;
    public float proactiveTopicIntervalSeconds = 10.0f;

    private int scenarioIndex;
    private int lineIndex = -1;
    private bool scenarioRunning;
    private float lastInteractionTime;
    private float lastProactiveTopicTime;
    private int proactiveReplyCursor;

    private static readonly ScriptedScenario[] Scenarios =
    {
        new ScriptedScenario(
            "Check-in",
            new ScriptedLine("user", "user_speech", "Hi Kyle, can you check in with me?"),
            new ScriptedLine("avatar", "avatar_reply", "Of course. I am here with you. Let's keep this simple and steady.", AvatarMotionCue.Salute),
            new ScriptedLine("user", "user_speech", "What should I pay attention to?"),
            new ScriptedLine("avatar", "avatar_reply", "Notice your breathing and whether your body feels calm, active, or tense."),
            new ScriptedLine("avatar", "avatar_reply", "I will keep watching the heart-rate stream and gently adjust the conversation.", AvatarMotionCue.Happy)
        ),
        new ScriptedScenario(
            "Grounding",
            new ScriptedLine("user", "user_speech", "I want to slow down for a moment."),
            new ScriptedLine("avatar", "avatar_reply", "Good choice. Look at me and let your shoulders drop a little.", AvatarMotionCue.Defeated),
            new ScriptedLine("avatar", "avatar_reply", "Breathe in for four counts, pause, and breathe out slowly."),
            new ScriptedLine("user", "user_speech", "That feels better."),
            new ScriptedLine("avatar", "avatar_reply", "Great. We can stay in this pace and let the scene remain quiet.", AvatarMotionCue.Happy)
        ),
        new ScriptedScenario(
            "Curiosity",
            new ScriptedLine("user", "user_speech", "Kyle, what are you noticing?"),
            new ScriptedLine("avatar", "avatar_reply", "I am noticing your current signal and the rhythm of this interaction.", AvatarMotionCue.Salute),
            new ScriptedLine("user", "user_speech", "Can you make this feel more conversational?"),
            new ScriptedLine("avatar", "avatar_reply", "Yes. I can ask small questions and react to your heart-rate zone without needing a live AI model."),
            new ScriptedLine("avatar", "avatar_reply", "This is a scripted demo, but the dashboard will still record the full conversation flow.", AvatarMotionCue.Happy)
        )
    };

    private static readonly ProactivePrompt[] CalmPrompts =
    {
        new ProactivePrompt("Your heart rate looks calm. Want to explore the scene at an easy pace?", AvatarMotionCue.Happy),
        new ProactivePrompt("You seem steady right now. I can start with a light check-in.", AvatarMotionCue.Salute),
        new ProactivePrompt("Your rhythm is relaxed. We can keep this conversation gentle."),
        new ProactivePrompt("This looks like a calm zone. What would you like to focus on?"),
        new ProactivePrompt("Your signal is quiet and stable. I will keep the tone open and simple.", AvatarMotionCue.Happy)
    };

    private static readonly ProactivePrompt[] ActivePrompts =
    {
        new ProactivePrompt("Your heart rate is active. I can keep the conversation responsive but not too intense.", AvatarMotionCue.Salute),
        new ProactivePrompt("You look engaged. Should we continue the current thread?"),
        new ProactivePrompt("Your signal has some energy. I can ask a short question and keep moving.", AvatarMotionCue.Happy),
        new ProactivePrompt("This is an active zone. I will keep my replies clear and brief."),
        new ProactivePrompt("Your body seems alert. Tell me if you want to slow the pace.", AvatarMotionCue.Salute)
    };

    private static readonly ProactivePrompt[] ElevatedPrompts =
    {
        new ProactivePrompt("Your heart rate is elevated. I can slow this down with you.", AvatarMotionCue.Defeated),
        new ProactivePrompt("I am seeing a higher signal. Let's take one steady breath before continuing."),
        new ProactivePrompt("Your body may be working harder right now. We can make this gentler.", AvatarMotionCue.Defeated),
        new ProactivePrompt("This looks elevated. I will keep the next step calm and grounded."),
        new ProactivePrompt("Let's pause for a second and check whether you feel okay.", AvatarMotionCue.Defeated)
    };

    private static readonly ProactivePrompt[] HighPrompts =
    {
        new ProactivePrompt("Your heart rate is high. Let's pause the story and focus on breathing.", AvatarMotionCue.Defeated),
        new ProactivePrompt("I am seeing a high zone. Stay still for a moment and exhale slowly.", AvatarMotionCue.Defeated),
        new ProactivePrompt("This is a strong signal. I will keep the interaction minimal and supportive."),
        new ProactivePrompt("Let's ground first: feet on the floor, eyes forward, slow breath out.", AvatarMotionCue.Defeated),
        new ProactivePrompt("Your heart rate is high, so I will stop asking questions and help you settle.")
    };

    private void Start()
    {
        lastInteractionTime = Time.time;
        lastProactiveTopicTime = Time.time;
        ShowReadyPrompt();
        StartCoroutine(ProactiveHeartRateTopics());
    }

    public void StartDemo()
    {
        scenarioIndex = 0;
        scenarioRunning = false;
        lineIndex = -1;
        lastInteractionTime = Time.time;
        lastProactiveTopicTime = Time.time;

        HeartRateSample sample = heartRateReceiver?.LatestSample;
        string message = sample == null
            ? "Demo started. Waiting for live Apple Watch heart-rate data."
            : $"Demo started. Latest heart rate is {sample.heartRate} bpm in the {sample.zone?.name ?? "unknown"} zone.";

        DisplayAndRecord("system", "system", "avatar", message, sample, AvatarMotionCue.Salute);
        StartCoroutine(PostDemoStartEvent(message));
    }

    public void StartCurrentScenario()
    {
        scenarioRunning = true;
        lineIndex = -1;
        lastInteractionTime = Time.time;
        NextLine();
    }

    public void SelectNextScenario()
    {
        scenarioIndex = (scenarioIndex + 1) % Scenarios.Length;
        scenarioRunning = false;
        lineIndex = -1;
        lastInteractionTime = Time.time;
        ShowReadyPrompt();
    }

    public void NextLine()
    {
        lastInteractionTime = Time.time;

        if (!scenarioRunning)
        {
            StartCurrentScenario();
            return;
        }

        ScriptedScenario scenario = Scenarios[scenarioIndex];
        lineIndex++;
        if (lineIndex >= scenario.Lines.Length)
        {
            scenarioRunning = false;
            lineIndex = -1;
            DisplayAndRecord("avatar", "avatar_reply", "user", $"{scenario.Name} is complete. Press X to replay or Y to switch.", null, AvatarMotionCue.Salute);
            return;
        }

        ScriptedLine line = scenario.Lines[lineIndex];
        DisplayAndRecord(line.Role, line.MessageType, "user", line.Text, null, line.MotionCue);
    }

    private void ShowReadyPrompt()
    {
        ScriptedScenario scenario = Scenarios[scenarioIndex];
        dialoguePanel?.SetReply($"Scene {scenarioIndex + 1}: {scenario.Name}. Press B to start demo. X starts, Y switches, A advances.");
    }

    private IEnumerator ProactiveHeartRateTopics()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            float now = Time.time;
            bool userHasBeenQuiet = now - lastInteractionTime >= proactiveTopicIntervalSeconds;
            bool enoughTimeSinceLastTopic = now - lastProactiveTopicTime >= proactiveTopicIntervalSeconds;
            if (userHasBeenQuiet && enoughTimeSinceLastTopic)
            {
                SubmitHeartRateTopic();
            }
        }
    }

    private void SubmitHeartRateTopic()
    {
        HeartRateSample sample = heartRateReceiver?.LatestSample;
        string zoneName = sample?.zone?.name ?? "unknown";
        ProactivePrompt prompt = PickProactivePrompt(zoneName);
        lastProactiveTopicTime = Time.time;
        DisplayAndRecord("avatar", "sensor_prompt", "avatar", prompt.Text, sample, prompt.MotionCue);
    }

    private ProactivePrompt PickProactivePrompt(string zoneName)
    {
        ProactivePrompt[] prompts = zoneName switch
        {
            "calm" => CalmPrompts,
            "active" => ActivePrompts,
            "elevated" => ElevatedPrompts,
            "high" => HighPrompts,
            _ => ActivePrompts
        };

        ProactivePrompt prompt = prompts[proactiveReplyCursor % prompts.Length];
        proactiveReplyCursor++;
        return prompt;
    }

    private void DisplayAndRecord(string role, string messageType, string initiator, string text, HeartRateSample sample, AvatarMotionCue motionCue)
    {
        dialoguePanel?.SetReply(text);
        motionController?.Play(motionCue);
        StartCoroutine(PostRecord(role, messageType, initiator, text, sample));
    }

    private IEnumerator PostRecord(string role, string messageType, string initiator, string text, HeartRateSample sample)
    {
        ChatRecordRequest payload = new ChatRecordRequest
        {
            role = role,
            text = text,
            messageType = messageType,
            conversationInitiator = initiator,
            heartRate = sample?.heartRate ?? 0,
            zone = sample?.zone?.name ?? string.Empty
        };

        string json = JsonUtility.ToJson(payload);
        using UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/api/chat/records", "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
    }

    private IEnumerator PostDemoStartEvent(string text)
    {
        DemoEventRequest payload = new DemoEventRequest
        {
            type = "demo_start",
            text = text,
            source = "quest_3"
        };

        string json = JsonUtility.ToJson(payload);
        using UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/api/demo/start", "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
    }

    private readonly struct ScriptedLine
    {
        public ScriptedLine(string role, string messageType, string text, AvatarMotionCue motionCue = AvatarMotionCue.None)
        {
            Role = role;
            MessageType = messageType;
            Text = text;
            MotionCue = motionCue;
        }

        public string Role { get; }
        public string MessageType { get; }
        public string Text { get; }
        public AvatarMotionCue MotionCue { get; }
    }

    private readonly struct ProactivePrompt
    {
        public ProactivePrompt(string text, AvatarMotionCue motionCue = AvatarMotionCue.None)
        {
            Text = text;
            MotionCue = motionCue;
        }

        public string Text { get; }
        public AvatarMotionCue MotionCue { get; }
    }

    private readonly struct ScriptedScenario
    {
        public ScriptedScenario(string name, params ScriptedLine[] lines)
        {
            Name = name;
            Lines = lines;
        }

        public string Name { get; }
        public ScriptedLine[] Lines { get; }
    }
}
