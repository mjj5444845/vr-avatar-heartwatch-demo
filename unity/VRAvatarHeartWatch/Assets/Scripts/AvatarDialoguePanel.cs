using UnityEngine;
using UnityEngine.UI;

public class AvatarDialoguePanel : MonoBehaviour
{
    public Text replyText;

    public void SetReply(string reply)
    {
        if (replyText != null)
        {
            replyText.text = reply;
        }
    }
}

