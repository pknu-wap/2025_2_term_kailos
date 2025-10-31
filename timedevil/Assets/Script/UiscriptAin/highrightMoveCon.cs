using UnityEngine;

public class InventoryCursor : MonoBehaviour
{
    [Header("�ʼ� ����")]
    [SerializeField] private RectTransform highlight;

    [Header("�� ����")]
    [SerializeField] private int rowCount = 6;
    [SerializeField] private float rowHeight = 60f;
    [SerializeField] private Vector2 topAnchoredPos = Vector2.zero;

    [Header("����")]
    [SerializeField] private int currentIndex = 0;

    private void Reset()
    {
        if (!highlight) highlight = GetComponent<RectTransform>();
    }

    private void Start()
    {
        SetToIndex(currentIndex);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) Move(-1);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) Move(+1);
    }

    private void Move(int delta)
    {
        int next = Mathf.Clamp(currentIndex + delta, 0, rowCount - 1);
        if (next == currentIndex) return;
        currentIndex = next;
        SetToIndex(currentIndex);
    }

    private void SetToIndex(int index)
    {
        Vector2 pos = topAnchoredPos + new Vector2(0f, -rowHeight * index);
        highlight.anchoredPosition = pos;
    }

    // �� �������� �ٲ� �� ȣ��: �� ��ĭ���� �̵�
    public void ResetToTop()
    {
        currentIndex = 0;
        SetToIndex(currentIndex);
    }
}
