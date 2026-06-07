using Meta.XR.BuildingBlocks.AIBlocks;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestVoiceInputController : MonoBehaviour
{
    public LlmConversationController conversationController;
    public SpeechToTextAgent speechToTextAgent;
    public InputActionProperty rightAButton;
    public Key editorFallbackKey = Key.Space;

    [TextArea]
    public string editorTestTranscript = "How do I feel right now?";

    private bool wasEditorFallbackPressed;

    private void Reset()
    {
        rightAButton = new InputActionProperty(new InputAction(
            "Quest Right A",
            InputActionType.Button,
            "<XRController>{RightHand}/primaryButton"));
    }

    private void Awake()
    {
        if (rightAButton.action == null)
        {
            rightAButton = new InputActionProperty(new InputAction(
                "Quest Right A",
                InputActionType.Button,
                "<XRController>{RightHand}/primaryButton"));
        }

        if (speechToTextAgent == null)
        {
            speechToTextAgent = FindAnyObjectByType<SpeechToTextAgent>();
        }
    }

    private void OnEnable()
    {
        if (rightAButton.action != null)
        {
            rightAButton.action.Enable();
        }

        if (speechToTextAgent != null)
        {
            speechToTextAgent.onTranscript.AddListener(SubmitMetaVoiceTranscript);
        }
    }

    private void OnDisable()
    {
        if (speechToTextAgent != null)
        {
            speechToTextAgent.onTranscript.RemoveListener(SubmitMetaVoiceTranscript);
            speechToTextAgent.StopNow();
        }

        if (rightAButton.action != null)
        {
            rightAButton.action.Disable();
        }
    }

    private void Update()
    {
        if (rightAButton.action != null && rightAButton.action.WasPressedThisFrame())
        {
            StartListening();
        }

        Keyboard keyboard = Keyboard.current;
        bool editorPressed = keyboard != null && keyboard[editorFallbackKey].isPressed;
        if (editorPressed && !wasEditorFallbackPressed)
        {
            StartListening();
        }

        wasEditorFallbackPressed = editorPressed;
    }

    public void StartListening()
    {
        if (speechToTextAgent != null)
        {
            speechToTextAgent.StartListening();
        }
        else
        {
            SubmitEditorTestTranscript();
        }
    }

    public void SubmitEditorTestTranscript()
    {
        conversationController?.SubmitTranscript(editorTestTranscript);
    }

    public void SubmitMetaVoiceTranscript(string transcript)
    {
        conversationController?.SubmitTranscript(transcript);
    }
}
