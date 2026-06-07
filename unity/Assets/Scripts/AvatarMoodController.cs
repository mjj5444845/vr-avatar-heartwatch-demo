using UnityEngine;

public class AvatarMoodController : MonoBehaviour
{
    public HeartRateReceiver receiver;
    public Renderer[] avatarRenderers;
    public Color calmColor = new Color(0.14f, 0.71f, 0.49f);
    public Color activeColor = new Color(0.85f, 0.66f, 0.0f);
    public Color elevatedColor = new Color(0.98f, 0.45f, 0.09f);
    public Color highColor = new Color(0.90f, 0.28f, 0.30f);

    private void OnEnable()
    {
        if (receiver != null)
        {
            receiver.OnSampleUpdated += ApplyHeartRateMood;
        }
    }

    private void OnDisable()
    {
        if (receiver != null)
        {
            receiver.OnSampleUpdated -= ApplyHeartRateMood;
        }
    }

    private void ApplyHeartRateMood(HeartRateSample sample)
    {
        Color color = sample.zone.name switch
        {
            "calm" => calmColor,
            "active" => activeColor,
            "elevated" => elevatedColor,
            "high" => highColor,
            _ => calmColor
        };

        foreach (Renderer avatarRenderer in avatarRenderers)
        {
            if (avatarRenderer != null)
            {
                avatarRenderer.material.color = color;
            }
        }
    }
}

