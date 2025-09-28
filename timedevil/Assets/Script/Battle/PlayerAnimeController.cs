using System.Collections;
using UnityEngine;

public class PlayerAnimeController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;   // Player_Stone

    [Header("Default Ease")]
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public bool IsPlaying { get; private set; }
    Coroutine playingCR;

    public void SetTarget(Transform t) => target = t;

    public void StopAll()
    {
        if (playingCR != null) StopCoroutine(playingCR);
        playingCR = null;
        IsPlaying = false;
    }

    /// <summary>start → end로 duration 동안 보간</summary>
    public Coroutine AnimateTo(Vector3 end, float duration, AnimationCurve customEase = null)
    {

    }

    /// <summary>start → end → start 왕복(halfDuration*2) + holdAtEnd</summary>
    public Coroutine AnimatePingPong(Vector3 end, float halfDuration, float holdAtEnd, AnimationCurve customEase = null)
    {

    }

    IEnumerator Co_Move(Vector3 start, Vector3 end, float duration, AnimationCurve customEase)
    {

    }

    IEnumerator Co_PingPong(Vector3 start, Vector3 end, float half, float hold, AnimationCurve customEase)
    {

    }
}
