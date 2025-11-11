// AttackCardSO.cs
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Attack Card", fileName = "AttackCard")]
public class AttackCardSO : BaseCardSO
{
    [Header("Attack")]
    public int power = 1;

    // ▼▼ 기존 단일 패턴(하위호환 유지) ▼▼
    [Tooltip("16자리 패턴: 위→아래, 각 행 4칸. 예) 1000 0100 0010 0001 → '1000010000100001'")]
    public string pattern16 = "0000000000000000";

    [Tooltip("타임라인(16개, 초 단위). 비우면 0으로 간주")]
    public float[] timeline = new float[16];

    [Header("FX")]
    public bool cameraShake = false;
    public string animationKey;      // 나중에 애니메이션 키로 사용
    public AudioClip sfx;

    // =========================
    //        Multi Waves
    // =========================

    [Header("Multi-Wave Pattern")]
    [Tooltip("켜면 아래 Waves 정의를 사용하고, 끄면 위의 단일 pattern16/timeline을 사용합니다.")]
    public bool useWaves = false;

    public enum WaveCombineMode
    {
        Replace,   // 이 웨이브의 pattern16이 그대로 적용
        Additive   // 이전 웨이브들의 누적 마스크에 이번 pattern16을 합집합(OR)으로 더함
    }

    [Serializable]
    public class AttackWave
    {
        [Tooltip("이 웨이브의 16자리 패턴")]
        public string pattern16 = "0000000000000000";

        [Tooltip("이 웨이브의 타임라인(16칸). 길이가 0이면 모두 0초로 간주")]
        public float[] timeline = new float[16];

        [Tooltip("이 웨이브를 이전 웨이브 결과 위에 누적(Additive)할지, 교체(Replace)할지")]
        public WaveCombineMode combine = WaveCombineMode.Replace;

        [Tooltip("이 웨이브 시작 전 대기 시간(초). 웨이브 간 템포 조절용")]
        public float preDelay = 0f;

        [Tooltip("이 웨이브가 끝난 뒤 다음 웨이브까지의 대기 시간(초)")]
        public float postDelay = 0f;

        [Tooltip("이 웨이브를 몇 번 반복할지 (각 반복은 같은 패턴/타임라인로 진행)")]
        public int repeat = 1;
    }

    [Tooltip("앞에서부터 순차적으로 재생될 웨이브들")]
    public AttackWave[] waves;

    // =========================
    //     편의 유틸리티(런타임)
    // =========================

    /// <summary>
    /// pattern16을 16칸 bool[16]로 파싱 (true=공격 타일)
    /// index = r*4 + c, r:0..3(상→하), c:0..3(좌→우)
    /// </summary>
    public static void ParsePattern16(string pattern, bool[] outMask16)
    {
        if (outMask16 == null || outMask16.Length != 16) return;

        for (int i = 0; i < 16; i++)
        {
            char ch = (pattern != null && pattern.Length > i) ? pattern[i] : '0';
            outMask16[i] = (ch == '1');
        }
    }

    /// <summary>
    /// 타임라인을 타일당 시간 배열(길이 16)로 채워준다. 부족하면 0으로 채움.
    /// </summary>
    public static void FillTimeline16(float[] src, float[] outTimes16)
    {
        if (outTimes16 == null || outTimes16.Length != 16) return;

        for (int i = 0; i < 16; i++)
        {
            float t = (src != null && src.Length > i) ? src[i] : 0f;
            outTimes16[i] = t;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 문자열 길이 강제는 하지 않고, 경고만 띄우는 쪽이 안전.
        if (!string.IsNullOrEmpty(pattern16) && pattern16.Length < 16)
        {
            // Unity 콘솔 경고
            Debug.LogWarning($"[AttackCardSO] pattern16 length < 16 on '{name}'. Shorter bits are treated as '0'.");
        }

        // waves 검증
        if (waves != null)
        {
            foreach (var w in waves)
            {
                if (w == null) continue;
                if (!string.IsNullOrEmpty(w.pattern16) && w.pattern16.Length < 16)
                    Debug.LogWarning($"[AttackCardSO] wave.pattern16 length < 16 on '{name}'.");
                if (w.repeat < 1) w.repeat = 1;
                if (w.preDelay < 0f) w.preDelay = 0f;
                if (w.postDelay < 0f) w.postDelay = 0f;
            }
        }
    }
#endif
}
