using UnityEngine;

public class QuestVoiceInputController : MonoBehaviour
{
    public LlmConversationController conversationController;

    [TextArea]
    public string editorTestTranscript = "How do I feel right now?";

    public void SubmitEditorTestTranscript()
    {
        conversationController?.SubmitTranscript(editorTestTranscript);
    }

    public void SubmitMetaVoiceTranscript(string transcript)
    {
        conversationController?.SubmitTranscript(transcript);
    }
}

