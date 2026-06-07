using UnityEngine;
using UnityEngine.UI;

public class HeartRateWorldPanel : MonoBehaviour
{
    public HeartRateReceiver receiver;
    public Text heartRateText;
    public Text zoneText;

    private void OnEnable()
    {
        if (receiver != null)
        {
            receiver.OnSampleUpdated += UpdatePanel;
        }
    }

    private void OnDisable()
    {
        if (receiver != null)
        {
            receiver.OnSampleUpdated -= UpdatePanel;
        }
    }

    private void Start()
    {
        if (heartRateText != null)
        {
            heartRateText.text = "-- bpm";
        }
        if (zoneText != null)
        {
            zoneText.text = "waiting for Apple Watch";
        }
    }

    private void UpdatePanel(HeartRateSample sample)
    {
        if (heartRateText != null)
        {
            heartRateText.text = $"{sample.heartRate} bpm";
        }
        if (zoneText != null)
        {
            zoneText.text = $"{sample.zone.name} · {sample.zone.tone}";
        }
    }
}

