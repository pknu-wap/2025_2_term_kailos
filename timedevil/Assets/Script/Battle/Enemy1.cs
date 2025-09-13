using UnityEngine;

public class Enemy1 : MonoBehaviour, ICardPattern
{
    [SerializeField] private string cardImagePath = "my_asset/Enemy1";
    [SerializeField] private string pattern16 = "1010101010000000";

    // 16칸의 발동 시간 (예: 전부 0초 → 동시에 발동)
    [SerializeField] private float[] timings = new float[16]
    {
        1f, 2f, 3f, 0f,
        5f, 6f, 7f, 0f,
        9f, 0f, 0f, 0f,
        13f, 0f, 0f, 0f
    };

    public string CardImagePath => cardImagePath;
    public string Pattern16 => pattern16;
    public float[] Timings => timings;
}
