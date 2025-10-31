using UnityEngine;
using UnityEngine.UI; // Text ��� ��
// using TMPro; // ���� TMP_Text�� ���ٸ� �ּ� ����

public class InventoryPageManagerKeys : MonoBehaviour
{
    [Header("������ ������Ʈ (�ϳ��� Ȱ��ȭ)")]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;

    [Header("������ �ؽ�Ʈ(����)")]
    [SerializeField] private Text pageText; // TMP��� TMP_Text�� �ٲټ���
    // [SerializeField] private TMP_Text pageText;

    [Header("Ŀ�� ��Ʈ�ѷ�")]
    [SerializeField] private InventoryCursor cursor;

    private int currentPage = 1;   // 1 �Ǵ� 2
    private const int totalPages = 2;

    private void Start()
    {
        ApplyPage(currentPage, resetCursor: true);
    }

    private void Update()
    {
        // �� ���� ������
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                ApplyPage(currentPage, resetCursor: true);
                Debug.Log($"{currentPage} �������� �̵�");
            }
            else
            {
                // ������ ������������ ����
                // Debug.Log("������ �̵� �Ұ� (������ ������)");
            }
        }

        // �� ���� ������
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentPage > 1)
            {
                currentPage--;
                ApplyPage(currentPage, resetCursor: true);
                Debug.Log($"{currentPage} �������� �̵�");
            }
            else
            {
                // ù ������������ ����
                // Debug.Log("���� �̵� �Ұ� (ù ������)");
            }
        }
    }

    private void ApplyPage(int page, bool resetCursor)
    {
        if (page1) page1.SetActive(page == 1);
        if (page2) page2.SetActive(page == 2);

        if (pageText) pageText.text = $"{page} / {totalPages}";

        if (resetCursor && cursor != null)
            cursor.ResetToTop();
    }
}
