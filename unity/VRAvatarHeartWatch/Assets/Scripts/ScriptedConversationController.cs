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

public class ScriptedConversationController : MonoBehaviour
{
    public string apiBaseUrl = "http://127.0.0.1:8787";
    public AvatarDialoguePanel dialoguePanel;
    public HeartRateReceiver heartRateReceiver;
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
            new ScriptedLine("avatar", "avatar_reply", "Of course. I am here with you. Let's keep this simple and steady."),
            new ScriptedLine("user", "user_speech", "What should I pay attention to?"),
            new ScriptedLine("avatar", "avatar_reply", "Notice your breathing and whether your body feels calm, active, or tense."),
            new ScriptedLine("avatar", "avatar_reply", "I will keep watching the heart-rate stream and gently adjust the conversation.")
        ),
        new ScriptedScenario(
            "Grounding",
            new ScriptedLine("user", "user_speech", "I want to slow down for a moment."),
            new ScriptedLine("avatar", "avatar_reply", "Good choice. Look at me and let your shoulders drop a little."),
            new ScriptedLine("avatar", "avatar_reply", "Breathe in for four counts, pause, and breathe out slowly."),
            new ScriptedLine("user", "user_speech", "That feels better."),
            new ScriptedLine("avatar", "avatar_reply", "Great. We can stay in this pace and let the scene remain quiet.")
        ),
        new ScriptedScenario(
            "Curiosity",
            new ScriptedLine("user", "user_speech", "Kyle, what are you noticing?"),
            new ScriptedLine("avatar", "avatar_reply", "I am noticing your current signal and the rhythm of this interaction."),
            new ScriptedLine("user", "user_speech", "Can you make this feel more conversational?"),
            new ScriptedLine("avatar", "avatar_reply", "Yes. I can ask small questions and react to your heart-rate zone without needing a live AI model."),
            new ScriptedLine("avatar", "avatar_reply", "This is a scripted demo, but the dashboard will still record the full conversation flow.")
        )
    };

    private static readonly string[] CalmPrompts =
    {
        "Your heart rate looks calm. Want to explore the scene at an easy pace?",
        "You seem steady right now. I can start with a light check-in.",
        "Your rhythm is relaxed. We can keep this conversation gentle.",
        "This looks like a calm zone. What would you like to focus on?",
        "Your signal is quiet and stable. I will keep the tone open and simple."
    };

    private static readonly string[] ActivePrompts =
    {
        "Your heart rate is active. I can keep the conversation responsive but not too intense.",
        "You look engaged. Should we continue the current thread?",
        "Your signal has some energy. I can ask a short question and keep moving.",
        "This is an active zone. I will keep my replies clear and brief.",
        "Your body seems alert. Tell me if you want to slow the pace."
    };

    private static readonly string[] ElevatedPrompts =
    {
        "Your heart rate is elevated. I can slow this down with you.",
        "I am seeing a higher signal. Let's take one steady breath before continuing.",
        "Your body may be working harder right now. We can make this gentler.",
        "This looks elevated. I will keep the next step calm and grounded.",
        "Let's pause for a second and check whether you feel okay."
    };

    private static readonly string[] HighPrompts =
    {
        "Your heart rate is high. Let's pause the story and focus on breathing.",
        "I am seeing a high zone. Stay still for a moment and exhale slowly.",
        "This is a strong signal. I will keep the interaction minimal and supportive.",
        "Let's ground first: feet on the floor, eyes forward, slow breath out.",
        "Your heart rate is high, so I will stop asking questions and help you settle."
    };

    private void Start()
    {
        lastInteractionTime = Time.time;
        lastProactiveTopicTime = Time.time;
        ShowReadyPrompt();
        StartCoroutine(ProactiveHeartRateTopics());
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
            DisplayAndRecord("avatar", "avatar_reply", "user", $"{scenario.Name} is complete. Press X to replay or Y to switch.", null);
            return;
        }

        ScriptedLine line = scenario.Lines[lineIndex];
        DisplayAndRecord(line.Role, line.MessageType, "user", line.Text, null);
    }

    private void ShowReadyPrompt()
    {
        ScriptedScenario scenario = Scenarios[scenarioIndex];
        dialoguePanel?.SetReply($"Scene {scenarioIndex + 1}: {scenario.Name}. Press X to start, Y to switch, A for next.");
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
        string text = PickProactivePrompt(zoneName);
        lastProactiveTopicTime = Time.time;
        DisplayAndRecord("avatar", "sensor_prompt", "avatar", text, sample);
    }

    private string PickProactivePrompt(string zoneName)
    {
        string[] prompts = zoneName switch
        {
            "calm" => CalmPrompts,
            "active" => ActivePrompts,
            "elevated" => ElevatedPrompts,
            "high" => HighPrompts,
            _ => ActivePrompts
        };

        string prompt = prompts[proactiveReplyCursor % prompts.Length];
        proactiveReplyCursor++;
        return prompt;
    }

    private void DisplayAndRecord(string role, string messageType, string initiator, string text, HeartRateSample sample)
    {
        dialoguePanel?.SetReply(text);
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

    private readonly struct ScriptedLine
    {
        public ScriptedLine(string role, string messageType, string text)
        {
            Role = role;
            MessageType = messageType;
            Text = text;
        }

        public string Role { get; }
        public string MessageType { get; }
        public string Text { get; }
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
