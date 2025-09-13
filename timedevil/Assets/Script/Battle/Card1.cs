using UnityEngine;

public class Card1 : MonoBehaviour, ICardPattern
{
    [SerializeField] private string cardImagePath = "my_asset/Card1";
    [SerializeField] private string pattern16 = "1111000011110000";

    // 16칸의 발동 시간 (예: 0=즉시, 1=1초 뒤, 2=2초 뒤 ...)
    [SerializeField] private float[] timings = new float[16]
    {
        0f, 1f, 2f, 3f,
        0f, 1f, 2f, 3f,
        0f, 1f, 2f, 3f,
        0f, 1f, 2f, 3f
    };

    public string CardImagePath => cardImagePath;
    public string Pattern16 => pattern16;
    public float[] Timings => timings;
}
