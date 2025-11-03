// Assets/Script/Battle/Card_script/ShowCardController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShowCardController : MonoBehaviour
{
    [Header("Target (assign explicitly in Inspector)")]
    [SerializeField] private Image targetImage;          // ShowCard 전용 Image (반드시 직접 할당)
    [SerializeField] private string resourcesFolder = "my_asset";

    [Header("Timing")]
    [Tooltip("WaitForSecondsRealtime 사용 여부 (true 권장)")]
    [SerializeField] private bool useUnscaledTime = true;

    private void Awake()
    {
        if (!targetImage)
        {
            Debug.LogError("[ShowCard] targetImage가 비어 있습니다. Inspector에서 전용 Image를 지정하세요.");
            enabled = false;
            return;
        }
        targetImage.enabled = false; // 시작 시 숨김
        targetImage.raycastTarget = false; // 프리뷰용이므로 클릭 막지 않도록
    }

    /// <summary>
    /// 카드 ID로 스프라이트를 로드해서 seconds 동안만 표시.
    /// 다른 UI에는 일절 손대지 않는다.
    /// </summary>
    public IEnumerator ShowForSecondsById(string id, float seconds)
    {
        if (!targetImage) yield break;

        Sprite sp = (!string.IsNullOrEmpty(id))
            ? Resources.Load<Sprite>($"{resourcesFolder}/{id}")
            : null;

        if (!sp)
        {
            Debug.LogWarning($"[ShowCard] Sprite not found for id='{id}' (path='{resourcesFolder}/{id}')");
            yield break; // 스프라이트 없으면 아무 것도 안 함
        }

        // 시작 로그 & 표시
        Debug.Log($"[ShowCard] SHOW id='{id}' for {seconds:0.##}s");
        targetImage.sprite = sp;
        targetImage.enabled = true;

        // 대기
        float t = Mathf.Max(0f, seconds);
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(t);
        else
            yield return new WaitForSeconds(t);

        // 종료 로그 & 숨김 (본인만)
        targetImage.enabled = false;
        targetImage.sprite = null;
        Debug.Log("[ShowCard] HIDE");
    }

    /// <summary>
    /// 이미 가진 스프라이트를 seconds 동안만 표시 (디버그/툴링용)
    /// </summary>
    public IEnumerator ShowForSeconds(Sprite sp, float seconds)
    {
        if (!targetImage || !sp) yield break;

        Debug.Log($"[ShowCard] SHOW (sprite) for {seconds:0.##}s");
        targetImage.sprite = sp;
        targetImage.enabled = true;

        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, seconds));
        else
            yield return new WaitForSeconds(Mathf.Max(0f, seconds));

        targetImage.enabled = false;
        targetImage.sprite = null;
        Debug.Log("[ShowCard] HIDE");
    }
}
