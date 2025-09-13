using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    // 어느 보드(패널)에 표시할지
    public enum Panel { Player, Enemy }

    [Header("Marker (Resources 경로)")]
    [Tooltip("Resources 폴더 기준 스프라이트 경로 (예: my_asset/attack)")]
    public string markerSpritePath = "my_asset/attack";

    [Tooltip("SpriteRenderer.sortingOrder")]
    public int sortingOrder = 40;

    [Tooltip("생성되는 마커 Z 좌표 고정값 (보드 위로 보이도록 -2 권장)")]
    public float zOverride = -2f;

    [Header("Fade (sec)")]
    public float fadeIn  = 0.6f;
    public float hold    = 0.1f;
    public float fadeOut = 0.6f;

    [Header("좌표(왼/오 패널 각 16칸, 인덱스 0~15)")]
    [Tooltip("플레이어(왼쪽) 보드의 16칸 월드 좌표")]
    public List<Vector3> playerPanelWorld = new List<Vector3>(16);

    [Tooltip("적(오른쪽) 보드의 16칸 월드 좌표")]
    public List<Vector3> enemyPanelWorld  = new List<Vector3>(16);

    // 내부 관리
    private readonly List<SpriteRenderer> _spawned = new();

    // ------------------------ Public API ------------------------

    /// <summary>
    /// 모든 마커 즉시 제거 + 코루틴 정지
    /// </summary>
    public void ClearAllImmediate()
    {
        StopAllCoroutines();
        foreach (var sr in _spawned)
            if (sr) Destroy(sr.gameObject);
        _spawned.Clear();
    }

    /// <summary>
    /// 타이밍 배열을 고려한 총 연출 시간(가장 늦게 시작하는 칸의 delay + fadeIn + hold + fadeOut)
    /// </summary>
    public float GetSequenceDuration(float[] timings)
    {
        float maxDelay = (timings != null && timings.Length == 16) ? timings.Max() : 0f;
        return maxDelay + fadeIn + hold + fadeOut + 0.02f; // 약간의 여유
    }

    /// <summary>
    /// 패턴만 주고 표시(오버로드). 기본 패널은 Enemy(오른쪽), 모든 타이밍 0.
    /// </summary>
    public void ShowPattern(string pattern16)
    {
        var zeros = new float[16]; // 전부 0f
        ShowPattern(pattern16, zeros, Panel.Enemy);
    }

    /// <summary>
    /// 패턴 + 타이밍 + 패널 지정 표시
    /// </summary>
    public void ShowPattern(string pattern16, float[] timings, Panel panel)
    {
        if (string.IsNullOrEmpty(pattern16) || pattern16.Length != 16)
        {
            Debug.LogError("[AttackController] pattern16은 정확히 16글자여야 합니다.");
            return;
        }
        if (timings == null || timings.Length != 16)
        {
            Debug.LogWarning("[AttackController] timings가 비었거나 16이 아니라서 0으로 대체합니다.");
            timings = new float[16];
        }

        StartCoroutine(Co_ShowTimed(pattern16, timings, panel));
    }

    // ------------------------ Coroutines ------------------------

    private IEnumerator Co_ShowTimed(string pattern16, float[] timings, Panel panel)
    {
        ClearAllImmediate();

        var sprite = Resources.Load<Sprite>(markerSpritePath);
        if (!sprite)
        {
            Debug.LogError($"[AttackController] 마커 스프라이트를 찾지 못했습니다: {markerSpritePath}");
            yield break;
        }

        var cells = (panel == Panel.Player) ? playerPanelWorld : enemyPanelWorld;

        // 각 "1" 칸을 개별 코루틴으로 타이밍에 맞춰 생성
        for (int i = 0; i < 16; i++)
        {
            if (pattern16[i] != '1') continue;

            if (i >= cells.Count)
            {
                Debug.LogWarning($"[AttackController] 좌표 리스트에 인덱스 {i}가 없습니다.");
                continue;
            }

            StartCoroutine(Co_SpawnOne(sprite, cells[i], timings[i]));
        }

        // 전체 연출이 끝날 때까지 대기 후 자동 정리
        float total = GetSequenceDuration(timings);
        yield return new WaitForSeconds(total);

        ClearAllImmediate(); // 필요 없다면 이 줄 제거해서 마커 유지 가능
    }

    private IEnumerator Co_SpawnOne(Sprite sprite, Vector3 worldPos, float delay)
    {
        // 시작 지연
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // GO 생성
        var go = new GameObject("attack");
        go.transform.position = new Vector3(worldPos.x, worldPos.y, zOverride);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = sortingOrder;
        sr.color = new Color(1f, 1f, 1f, 0f); // alpha 0 시작
        _spawned.Add(sr);

        // Fade In
        if (fadeIn > 0f)
        {
            float t = 0f;
            while (t < fadeIn)
            {
                t += Time.deltaTime;
                SetAlpha(sr, Mathf.Clamp01(t / fadeIn));
                yield return null;
            }
        }
        else SetAlpha(sr, 1f);

        // Hold
        if (hold > 0f) yield return new WaitForSeconds(hold);

        // Fade Out
        if (fadeOut > 0f)
        {
            float t = 0f;
            while (t < fadeOut)
            {
                t += Time.deltaTime;
                SetAlpha(sr, 1f - Mathf.Clamp01(t / fadeOut));
                yield return null;
            }
        }
        else SetAlpha(sr, 0f);
    }

    // ------------------------ Helpers ------------------------

    private static void SetAlpha(SpriteRenderer sr, float a)
    {
        if (!sr) return;
        var c = sr.color;
        c.a = a;
        sr.color = c;
    }

#if UNITY_EDITOR
    // 에디터에서 16칸 보장하는 편의(선택)
    private void OnValidate()
    {
        if (playerPanelWorld.Count != 16)
        {
            while (playerPanelWorld.Count < 16) playerPanelWorld.Add(Vector3.zero);
            while (playerPanelWorld.Count > 16) playerPanelWorld.RemoveAt(playerPanelWorld.Count - 1);
        }
        if (enemyPanelWorld.Count != 16)
        {
            while (enemyPanelWorld.Count < 16) enemyPanelWorld.Add(Vector3.zero);
            while (enemyPanelWorld.Count > 16) enemyPanelWorld.RemoveAt(enemyPanelWorld.Count - 1);
        }
    }
#endif
}
