using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class HeartRateSample
{
    public string id;
    public string source;
    public int heartRate;
    public string timestamp;
    public HeartRateZone zone;
}

[Serializable]
public class HeartRateZone
{
    public string name;
    public string tone;
}

public class HeartRateReceiver : MonoBehaviour
{
    public string apiBaseUrl = "http://127.0.0.1:8787";
    public float pollIntervalSeconds = 2.0f;
    public HeartRateSample LatestSample { get; private set; }
    public bool IsPolling { get; private set; } = true;

    public event Action<HeartRateSample> OnSampleUpdated;

    private void Start()
    {
        StartCoroutine(PollLatestSample());
    }

    public void StopPolling()
    {
        IsPolling = false;
    }

    private IEnumerator PollLatestSample()
    {
        while (true)
        {
            if (!IsPolling)
            {
                yield break;
            }

            using UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/api/latest");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success && request.downloadHandler.text != "null")
            {
                LatestSample = JsonUtility.FromJson<HeartRateSample>(request.downloadHandler.text);
                OnSampleUpdated?.Invoke(LatestSample);
            }

            yield return new WaitForSeconds(pollIntervalSeconds);
        }
    }
}
