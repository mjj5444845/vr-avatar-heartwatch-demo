using UnityEngine;
using UnityEngine.InputSystem;

public class QuestScriptedInputController : MonoBehaviour
{
    public ScriptedConversationController conversationController;
    public InputActionProperty leftXButton;
    public InputActionProperty leftYButton;
    public InputActionProperty rightAButton;
    public InputActionProperty rightBButton;
    public InputActionProperty rightMenuButton;

    public Key editorStartKey = Key.X;
    public Key editorSwitchKey = Key.Y;
    public Key editorNextKey = Key.N;
    public Key editorNextAltKey = Key.Enter;
    public Key editorDemoStartKey = Key.B;
    public Key editorExitKey = Key.Q;

    private bool wasEditorStartPressed;
    private bool wasEditorSwitchPressed;
    private bool wasEditorNextPressed;
    private bool wasEditorDemoStartPressed;
    private bool wasEditorExitPressed;

    private void Reset()
    {
        leftXButton = CreateButtonAction("Quest Left X", "<XRController>{LeftHand}/primaryButton");
        leftYButton = CreateButtonAction("Quest Left Y", "<XRController>{LeftHand}/secondaryButton");
        rightAButton = CreateButtonAction("Quest Right A", "<XRController>{RightHand}/primaryButton");
        rightBButton = CreateButtonAction("Quest Right B", "<XRController>{RightHand}/secondaryButton");
        rightMenuButton = CreateButtonAction("Quest Right Menu", "<XRController>{RightHand}/menuButton");
    }

    private void Awake()
    {
        if (leftXButton.action == null)
        {
            leftXButton = CreateButtonAction("Quest Left X", "<XRController>{LeftHand}/primaryButton");
        }

        if (leftYButton.action == null)
        {
            leftYButton = CreateButtonAction("Quest Left Y", "<XRController>{LeftHand}/secondaryButton");
        }

        if (rightAButton.action == null)
        {
            rightAButton = CreateButtonAction("Quest Right A", "<XRController>{RightHand}/primaryButton");
        }

        if (rightBButton.action == null)
        {
            rightBButton = CreateButtonAction("Quest Right B", "<XRController>{RightHand}/secondaryButton");
        }

        if (rightMenuButton.action == null)
        {
            rightMenuButton = CreateButtonAction("Quest Right Menu", "<XRController>{RightHand}/menuButton");
        }
    }

    private void OnEnable()
    {
        leftXButton.action?.Enable();
        leftYButton.action?.Enable();
        rightAButton.action?.Enable();
        rightBButton.action?.Enable();
        rightMenuButton.action?.Enable();
    }

    private void OnDisable()
    {
        leftXButton.action?.Disable();
        leftYButton.action?.Disable();
        rightAButton.action?.Disable();
        rightBButton.action?.Disable();
        rightMenuButton.action?.Disable();
    }

    private void Update()
    {
        if (leftXButton.action != null && leftXButton.action.WasPressedThisFrame())
        {
            conversationController?.StartCurrentScenario();
        }

        if (leftYButton.action != null && leftYButton.action.WasPressedThisFrame())
        {
            conversationController?.SelectNextScenario();
        }

        if (rightAButton.action != null && rightAButton.action.WasPressedThisFrame())
        {
            conversationController?.NextLine();
        }

        if (rightBButton.action != null && rightBButton.action.WasPressedThisFrame())
        {
            conversationController?.StartDemo();
        }

        if (rightMenuButton.action != null && rightMenuButton.action.WasPressedThisFrame())
        {
            conversationController?.StopDemoAndQuit();
        }

        HandleEditorFallbackKeys();
    }

    private void HandleEditorFallbackKeys()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        bool startPressed = keyboard[editorStartKey].isPressed;
        bool switchPressed = keyboard[editorSwitchKey].isPressed;
        bool nextPressed = keyboard[editorNextKey].isPressed || keyboard[editorNextAltKey].isPressed;
        bool demoStartPressed = keyboard[editorDemoStartKey].isPressed;
        bool exitPressed = keyboard[editorExitKey].isPressed;

        if (exitPressed && !wasEditorExitPressed)
        {
            conversationController?.StopDemoAndQuit();
        }

        if (demoStartPressed && !wasEditorDemoStartPressed)
        {
            conversationController?.StartDemo();
        }

        if (startPressed && !wasEditorStartPressed)
        {
            conversationController?.StartCurrentScenario();
        }

        if (switchPressed && !wasEditorSwitchPressed)
        {
            conversationController?.SelectNextScenario();
        }

        if (nextPressed && !wasEditorNextPressed)
        {
            conversationController?.NextLine();
        }

        wasEditorStartPressed = startPressed;
        wasEditorSwitchPressed = switchPressed;
        wasEditorNextPressed = nextPressed;
        wasEditorDemoStartPressed = demoStartPressed;
        wasEditorExitPressed = exitPressed;
    }

    private static InputActionProperty CreateButtonAction(string name, string path)
    {
        return new InputActionProperty(new InputAction(name, InputActionType.Button, path));
    }
}
