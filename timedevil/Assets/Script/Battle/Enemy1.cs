using UnityEngine;

public class Enemy1 : MonoBehaviour, ICardPattern
{
    [SerializeField] private string cardImagePath = "my_asset/Enemy1";
    [SerializeField] private string pattern16 = "1111111111111111";

    // 16칸의 발동 시간 (예: 전부 0초 → 동시에 발동)
    [SerializeField] private float[] timings = new float[16]
    {
        0f, 0f, 0f, 0f,
        0f, 4f, 3f, 0f,
        0f, 1f, 2f, 0f,
        0f, 0f, 0f, 0f
    };

    public string CardImagePath => cardImagePath;
    public string Pattern16 => pattern16;
    public float[] Timings => timings;
}
