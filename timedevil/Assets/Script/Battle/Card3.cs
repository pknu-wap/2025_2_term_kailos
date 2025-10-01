using UnityEngine;

public class Card3 : MonoBehaviour, ICardPattern
{
    [SerializeField] private string cardImagePath = "my_asset/Card3";
    [SerializeField] private string pattern16 = "1100110011001100";

    // 16칸의 발동 시간 (예: 0=즉시, 1=1초 뒤, 2=2초 뒤 ...)
    [SerializeField] private float[] timings = new float[16]
    {
        0f, 0f, 0f, 0f,
        0f, 1f, 1f, 0f,
        0f, 1f, 1f, 0f,
        0f, 0f, 0f, 0f
    };

    public string CardImagePath => cardImagePath;
    public string Pattern16 => pattern16;
    public float[] Timings => timings;
}
