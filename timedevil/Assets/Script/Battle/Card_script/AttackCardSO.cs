// AttackCardSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Attack Card", fileName = "AttackCard")]
public class AttackCardSO : BaseCardSO
{
    [Header("Attack")]
    public int power = 1;

    [Tooltip("16자리 패턴: 위에서부터 행 4개 x 각 4칸, 예: 1000 0100 0010 0001 -> '1000010000100001'")]
    public string pattern16 = "0000000000000000";

    [Tooltip("타임라인(16개, 초 단위). 비워두면 0으로 간주")]
    public float[] timeline = new float[16];

    [Header("FX")]
    public bool cameraShake = false;
    public string animationKey;      // 나중에 애니메이션 재생키로 사용
    public AudioClip sfx;
}
