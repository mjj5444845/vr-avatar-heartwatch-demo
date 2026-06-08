using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
            "Status Check",
            new ScriptedLine("user", "user_speech", "Kyle, can you check my current state?"),
            new ScriptedLine("avatar", "avatar_reply", "Of course. I will keep this simple, steady, and watch your heart-rate changes.", AvatarMotionCue.Salute),
            new ScriptedLine("user", "user_speech", "What should I pay attention to?"),
            new ScriptedLine("avatar", "avatar_reply", "Notice your breathing, and whether your body feels calm, active, or a little tense."),
            new ScriptedLine("avatar", "avatar_reply", "I will adjust the dialogue rhythm based on your heart-rate zone.", AvatarMotionCue.Happy)
        ),
        new ScriptedScenario(
            "Slow Down",
            new ScriptedLine("user", "user_speech", "I want to slow down for a moment."),
            new ScriptedLine("avatar", "avatar_reply", "Good. Look toward me and let your shoulders soften a little.", AvatarMotionCue.Defeated),
            new ScriptedLine("avatar", "avatar_reply", "Breathe in for four counts, pause briefly, then breathe out slowly."),
            new ScriptedLine("user", "user_speech", "That feels a bit better."),
            new ScriptedLine("avatar", "avatar_reply", "Nice. We can keep that pace and let the scene stay quiet.", AvatarMotionCue.Happy)
        ),
        new ScriptedScenario(
            "Feedback",
            new ScriptedLine("user", "user_speech", "Kyle, what are you observing right now?"),
            new ScriptedLine("avatar", "avatar_reply", "I am watching your heart-rate signal and the pace of this interaction.", AvatarMotionCue.Salute),
            new ScriptedLine("user", "user_speech", "Can this feel more like a conversation?"),
            new ScriptedLine("avatar", "avatar_reply", "Yes. I will ask small scripted questions and react to the current heart-rate zone."),
            new ScriptedLine("avatar", "avatar_reply", "This is a lightweight demo, but the database records the full dialogue flow.", AvatarMotionCue.Happy)
        )
    };

    private static readonly ProactivePrompt[] CalmPrompts =
    {
        new ProactivePrompt("Your heart rate looks steady. Would you like to continue at an easy pace?", AvatarMotionCue.Happy),
        new ProactivePrompt("You look fairly stable right now. I can start with a simple check-in.", AvatarMotionCue.Salute),
        new ProactivePrompt("Your rhythm seems relaxed, so we can keep the dialogue gentle."),
        new ProactivePrompt("This looks like a calm zone. Where would you like to place your attention?"),
        new ProactivePrompt("The signal is steady, so I will keep my response simple and clear.", AvatarMotionCue.Happy)
    };

    private static readonly ProactivePrompt[] ActivePrompts =
    {
        new ProactivePrompt("Your heart rate is in an active zone. I will stay responsive without pushing the pace.", AvatarMotionCue.Salute),
        new ProactivePrompt("You seem engaged. Would you like to continue the current topic?"),
        new ProactivePrompt("The signal has some energy. I can ask one short question and keep moving.", AvatarMotionCue.Happy),
        new ProactivePrompt("This is an active zone, so I will make replies shorter and clearer."),
        new ProactivePrompt("Your body seems alert. If you want to slow down, you can tell me.", AvatarMotionCue.Salute)
    };

    private static readonly ProactivePrompt[] ElevatedPrompts =
    {
        new ProactivePrompt("Your heart rate is a little elevated. I can help slow the pace with you.", AvatarMotionCue.Defeated),
        new ProactivePrompt("I see a higher signal. Let us take one steady breath before continuing."),
        new ProactivePrompt("Your body may be working harder right now. We can make the interaction lighter.", AvatarMotionCue.Defeated),
        new ProactivePrompt("This is slightly elevated, so I will make the next step calmer and steadier."),
        new ProactivePrompt("Let us pause for a second and check whether you feel okay.", AvatarMotionCue.Defeated)
    };

    private static readonly ProactivePrompt[] HighPrompts =
    {
        new ProactivePrompt("Your heart rate is high. We will pause the story and focus on breathing.", AvatarMotionCue.Defeated),
        new ProactivePrompt("I see a high-zone signal. Stay still for a moment and breathe out slowly.", AvatarMotionCue.Defeated),
        new ProactivePrompt("This is a strong signal. I will reduce interaction and provide support."),
        new ProactivePrompt("Settle first: feet on the floor, eyes forward, and breathe out slowly.", AvatarMotionCue.Defeated),
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
            ? "Demo started. Waiting for live Apple Watch heart rate."
            : $"Demo started. Current heart rate is {sample.heartRate} bpm. Zone: {sample.zone?.name ?? "unknown"}.";

        DisplayAndRecord("system", "system", "avatar", message, sample, AvatarMotionCue.Salute);
        StartCoroutine(PostDemoStartEvent(message));
    }

    public void StopDemoAndQuit()
    {
        lastInteractionTime = Time.time;
        scenarioRunning = false;
        string message = "Demo exited. Heart-rate polling and live writes are paused.";
        dialoguePanel?.SetReply(message);
        heartRateReceiver?.StopPolling();
        StartCoroutine(PostDemoStopEventAndQuit(message));
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
            DisplayAndRecord("avatar", "avatar_reply", "user", $"{scenario.Name} complete. Press X to replay or Y to switch.", null, AvatarMotionCue.Salute);
            return;
        }

        ScriptedLine line = scenario.Lines[lineIndex];
        DisplayAndRecord(line.Role, line.MessageType, "user", line.Text, null, line.MotionCue);
    }

    private void ShowReadyPrompt()
    {
        ScriptedScenario scenario = Scenarios[scenarioIndex];
        dialoguePanel?.SetReply($"Scene {scenarioIndex + 1}: {scenario.Name}. B starts, A advances, X/Y switches, menu or Q exits.");
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

    private IEnumerator PostDemoStopEventAndQuit(string text)
    {
        DemoEventRequest payload = new DemoEventRequest
        {
            type = "demo_stop",
            text = text,
            source = "quest_3"
        };

        string json = JsonUtility.ToJson(payload);
        using UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/api/demo/stop", "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
