using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ChatRequest
{
    public string text;
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

    public void SubmitTranscript(string transcript)
    {
        if (!string.IsNullOrWhiteSpace(transcript))
        {
            StartCoroutine(PostTranscript(transcript));
        }
    }

    private IEnumerator PostTranscript(string transcript)
    {
        string json = JsonUtility.ToJson(new ChatRequest { text = transcript });
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
        }
        else
        {
            dialoguePanel?.SetReply("I could not reach the LLM bridge yet.");
        }
    }
}

