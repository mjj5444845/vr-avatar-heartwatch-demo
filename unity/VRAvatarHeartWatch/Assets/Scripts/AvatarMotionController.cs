using System.Collections;
using UnityEngine;

public enum AvatarMotionCue
{
    None,
    Salute,
    Happy,
    Defeated
}

public class AvatarMotionController : MonoBehaviour
{
    public Animator animator;
    public AnimationClip saluteClip;
    public AnimationClip happyClip;
    public AnimationClip defeatedClip;
    public string idleStateName = "Idle";
    public string saluteStateName = "Salute";
    public string happyStateName = "Happy";
    public string defeatedStateName = "Defeated";
    public float crossFadeSeconds = 0.12f;
    public float maxGestureSeconds = 3.0f;

    private Coroutine restoreCoroutine;

    public void Play(AvatarMotionCue cue)
    {
        AnimationClip clip = cue switch
        {
            AvatarMotionCue.Salute => saluteClip,
            AvatarMotionCue.Happy => happyClip,
            AvatarMotionCue.Defeated => defeatedClip,
            _ => null
        };

        if (animator == null || clip == null)
        {
            return;
        }

        string stateName = cue switch
        {
            AvatarMotionCue.Salute => saluteStateName,
            AvatarMotionCue.Happy => happyStateName,
            AvatarMotionCue.Defeated => defeatedStateName,
            _ => idleStateName
        };

        animator.CrossFadeInFixedTime(stateName, crossFadeSeconds);

        if (restoreCoroutine != null)
        {
            StopCoroutine(restoreCoroutine);
        }

        restoreCoroutine = StartCoroutine(RestoreIdleAfter(Mathf.Min(maxGestureSeconds, Mathf.Max(0.6f, clip.length))));
    }

    private IEnumerator RestoreIdleAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (animator != null)
        {
            animator.CrossFadeInFixedTime(idleStateName, crossFadeSeconds);
        }
    }
}
