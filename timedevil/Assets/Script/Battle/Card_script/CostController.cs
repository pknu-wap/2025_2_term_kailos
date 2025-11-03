// Assets/Script/Battle/CostController.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CostController : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private int max = 10;
    [SerializeField] private int current = 10;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text text;     // "AC 7/10" 같은 표시를 원하면 연결
    [SerializeField] private Slider slider;     // 게이지를 쓰면 연결

    public int Current => current;
    public int Max => max;

    void Start() => Refresh();

    /// <summary>현재 코스트를 정확히 value로 설정.</summary>
    public void ResetTo(int value)
    {
        current = Mathf.Clamp(value, 0, max);
        Refresh();
    }

    /// <summary>최대치로 리셋.</summary>
    public void ResetToMax()
    {
        current = max;
        Refresh();
    }

    /// <summary>코스트를 amount만큼 지불. 충분하면 차감하고 true, 부족하면 false.</summary>
    public bool TryPay(int amount)
    {
        if (amount <= 0) return true;
        if (current < amount) return false;
        current -= amount;
        Refresh();
        return true;
    }

    /// <summary>코스트 보충(+/- 모두 허용, 0~max로 클램프).</summary>
    public void Add(int amount)
    {
        current = Mathf.Clamp(current + amount, 0, max);
        Refresh();
    }

    private void Refresh()
    {
        if (text) text.text = $"AC {current}/{max}";
        if (slider)
        {
            slider.minValue = 0;
            slider.maxValue = max;
            slider.value = current;
        }
    }
}
