// Assets/Script/Battle/Card_script/ShowCardController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShowCardController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Image useImage;               // ✅ 씬의 UseImage 지정
    [SerializeField] private string resourcesFolder = "my_asset";

    [Header("Fade (sec)")]
    [SerializeField] private float fadeIn = 0.35f;
    [SerializeField] private float hold = 2.30f;
    [SerializeField] private float fadeOut = 0.35f;

    private CanvasGroup _cg;

    void Reset()
    {
        if (!useImage) useImage = GetComponentInChildren<Image>(true);
    }

    void Awake()
    {
        if (!useImage) useImage = GetComponentInChildren<Image>(true);

        // CanvasGroup 없으면 자동 추가 (페이드용)
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();

        // 시작은 숨김
        _cg.alpha = 0f;
        useImage.enabled = false;
        gameObject.SetActive(true); // 그룹 제어는 alpha로
    }

    /// <summary>
    /// 카드 ID로 스프라이트를 로드해서 페이드 인→홀드→아웃(총 약 3s) 재생.
    /// 다른 UI에는 손 안댐.
    /// </summary>
    public IEnumerator PreviewById(string id)
    {
        if (!useImage) yield break;

        var sp = string.IsNullOrEmpty(id) ? null : Resources.Load<Sprite>($"{resourcesFolder}/{id}");
        useImage.sprite = sp;
        useImage.enabled = sp != null;

        // 페이드 인
        yield return FadeTo(1f, Mathf.Max(0f, fadeIn));
        // 홀드
        yield return WaitForSecondsUnscaled(Mathf.Max(0f, hold));
        // 페이드 아웃
        yield return FadeTo(0f, Mathf.Max(0f, fadeOut));

        // 정리
        useImage.enabled = false;
        useImage.sprite = null;
    }

    private IEnumerator FadeTo(float target, float dur)
    {
        if (dur <= 0f) { _cg.alpha = target; yield break; }

        float start = _cg.alpha;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            _cg.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }
        _cg.alpha = target;
    }

    private static IEnumerator WaitForSecondsUnscaled(float s)
    {
        float t = s;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
