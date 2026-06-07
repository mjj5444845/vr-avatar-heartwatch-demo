using System;
using System.Collections;
using System.Text;
using Meta.XR.BuildingBlocks.AIBlocks;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ChatRequest
{
    public string text;
    public string messageType;
    public string conversationInitiator;
}

[Serializable]
public class ChatResponse
{
    public string transcript;
    public string reply;
    public int heartRate;
    public HeartRateZone zone;
}

public class LlmConversationController : MonoBehaviour
{
    public string apiBaseUrl = "http://127.0.0.1:8787";
    public AvatarDialoguePanel dialoguePanel;
    public HeartRateReceiver heartRateReceiver;
    public TextToSpeechAgent textToSpeechAgent;
    public float proactiveTopicIntervalSeconds = 10.0f;

    private bool requestInFlight;
    private float lastUserSpeechTime;
    private float lastProactiveTopicTime;

    private void Start()
    {
        lastUserSpeechTime = Time.time;
        lastProactiveTopicTime = Time.time;
        StartCoroutine(ProactiveHeartRateTopics());
    }

    public void SubmitTranscript(string transcript)
    {
        if (!string.IsNullOrWhiteSpace(transcript))
        {
            lastUserSpeechTime = Time.time;
            StartCoroutine(PostTranscript(transcript, "user_speech", "user", false));
        }
    }

    public void SubmitHeartRateTopic()
    {
        HeartRateSample sample = heartRateReceiver?.LatestSample;
        string heartContext = sample == null
            ? "The user has not spoken. Start a brief check-in because the heart-rate stream is not available yet."
            : $"The user has not spoken. Start a brief check-in based on the current heart rate: {sample.heartRate} bpm, zone: {sample.zone?.name ?? "unknown"}, tone: {sample.zone?.tone ?? "unknown"}.";

        StartCoroutine(PostTranscript(heartContext, "sensor_prompt", "avatar", true));
    }

    private IEnumerator ProactiveHeartRateTopics()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (requestInFlight)
            {
                continue;
            }

            float now = Time.time;
            bool userHasBeenQuiet = now - lastUserSpeechTime >= proactiveTopicIntervalSeconds;
            bool enoughTimeSinceLastTopic = now - lastProactiveTopicTime >= proactiveTopicIntervalSeconds;
            if (userHasBeenQuiet && enoughTimeSinceLastTopic)
            {
                SubmitHeartRateTopic();
            }
        }
    }

    private IEnumerator PostTranscript(string transcript, string messageType, string conversationInitiator, bool proactive)
    {
        requestInFlight = true;
        if (proactive)
        {
            lastProactiveTopicTime = Time.time;
        }

        string json = JsonUtility.ToJson(new ChatRequest
        {
            text = transcript,
            messageType = messageType,
            conversationInitiator = conversationInitiator
        });
        using UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/api/chat", "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ChatResponse response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
            dialoguePanel?.SetReply(response.reply);
            textToSpeechAgent?.SpeakText(response.reply);
        }
        else
        {
            dialoguePanel?.SetReply("I could not reach the LLM bridge yet.");
        }

        requestInFlight = false;
    }
}
