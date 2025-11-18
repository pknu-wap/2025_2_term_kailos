using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Attack Card", fileName = "AttackCard")]
public class AttackCardSO : BaseCardSO
{
    [Header("Attack")]
    public int power = 1;

    // ▼▼ 레거시 필드(호환용). 이제는 'waves가 비었을 때만' 참고하고, 보통은 무시됨 ▼▼
    [Tooltip("16자리 패턴: 위→아래, 각 행 4칸. 예) 1000 0100 0010 0001 → '1000010000100001'")]
    public string pattern16 = "0000000000000000";

    [Tooltip("타임라인(16개, 초 단위). 비우면 0으로 간주")]
    public float[] timeline = new float[16];

    [Header("Global FX (optional)")]
    public bool cameraShake = false;
    public string animationKey;
    public AudioClip sfx;                 // 전역 SFX(미사용 가능)

    // =========================
    //         Waves Only
    // =========================
    [Serializable]
    public class Wave
    {
        [Header("Pattern")]
        [Tooltip("이 웨이브의 16자리 패턴")]
        public string pattern16 = "0000000000000000";

        [Tooltip("이 웨이브의 타임라인(16칸). 길이가 0이면 모두 0초로 간주")]
        public float[] timeline = new float[16];

        [Header("Timing")]
        [Tooltip("이 웨이브 시작 전 지연(초)")]
        public float delayBefore = 0f;

        [Tooltip("이 웨이브가 끝난 뒤 다음 웨이브까지 지연(초)")]
        public float delayAfter = 0f;

        [Header("FX Hooks (optional)")]
        public AudioClip sfx;             // 웨이브용 SFX
        public bool sfxEveryHit = true;   // true면 각 타점마다, false면 웨이브 시작 시 1회
        public GameObject vfxPrefab;      // 히트 지점에 생성할 VFX
        public bool vfxEveryHit = true;   // true면 각 타점마다, false면 웨이브 시작 지점 1회
        public float vfxLifetime = 0.6f;  // 생성 VFX 자동 파괴 시간
    }

    [Tooltip("앞에서부터 순차적으로 재생될 웨이브들")]
    public Wave[] waves;

    // =========================
    //     런타임 유틸
    // =========================
    public static void ParsePattern16(string pattern, bool[] outMask16)
    {
        if (outMask16 == null || outMask16.Length != 16) return;
        for (int i = 0; i < 16; i++)
        {
            char ch = (pattern != null && pattern.Length > i) ? pattern[i] : '0';
            outMask16[i] = (ch == '1');
        }
    }

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
        if (waves != null)
        {
            foreach (var w in waves)
            {
                if (w == null) continue;
                if (w.delayBefore < 0f) w.delayBefore = 0f;
                if (w.delayAfter < 0f) w.delayAfter = 0f;
                if (w.vfxLifetime < 0f) w.vfxLifetime = 0f;
            }
        }
    }
#endif
}
