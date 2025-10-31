using UnityEngine;
using UnityEngine.UI;

public class HandSelectController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BattleMenuController menu; // ��Ŀ��/�Է� �ҽ�
    [SerializeField] private HandUI hand;               // ī�� �������� �׸��� UI
    [SerializeField] private Image selector;            // ������ �׵θ� �̹���(Ȱ��/��ġ �̵�)

    [Header("Behavior")]
    [SerializeField] private bool wrap = true;          // ��迡�� ���� �̵�

    private bool selecting = false;
    private int selIndex = 0;

    void Reset()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!hand) hand = FindObjectOfType<HandUI>(true);
    }

    void Awake()
    {
        if (selector) selector.enabled = false;
    }

    void Update()
    {
        // ���� ��尡 �ƴ� ��: Card�� ��Ŀ���� ���¿��� E�� ����
        if (!selecting)
        {
            if (menu && menu.Index == 0 && Input.GetKeyDown(KeyCode.E))
                EnterSelectMode();
            return;
        }

        // === ���� ��� === (�޴� �Է��� ���� ����)
        if (hand == null || hand.CardCount == 0)
        {
            // ���а� ������ �ڵ� ����
            ExitSelectMode();
            return;
        }

        // ��/�� �̵�
        if (Input.GetKeyDown(KeyCode.RightArrow))
            Move(+1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            Move(-1);

        // Q: ���� ���(���� ��� ����)
        if (Input.GetKeyDown(KeyCode.Q))
            ExitSelectMode();

        // E: Ȯ��(���⼭�� �α׸� ����� ����; �� ������ ���� �� ������)
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[HandSelect] Use hand index = {selIndex}");
            // TODO: BattleDeckRuntime.Instance.UseCardToBottom(selIndex) ���� ���� ��� ���� ����
            ExitSelectMode();
        }
    }

    private void EnterSelectMode()
    {
        if (hand == null || hand.CardCount == 0) return;

        selecting = true;
        selIndex = Mathf.Clamp(selIndex, 0, hand.CardCount - 1);

        // �޴� �Է� OFF
        if (menu) menu.EnableInput(false);

        // ���� �ڽ� ǥ�� + ��ġ ���߱�
        if (selector)
        {
            selector.enabled = true;
            SnapSelectorTo(selIndex);
        }

        Debug.Log("[Bridge] Hand select mode entered.");
    }

    private void ExitSelectMode()
    {
        selecting = false;

        // ���� �ڽ� ����
        if (selector) selector.enabled = false;

        // �޴� �Է� ON
        if (menu) menu.EnableInput(true);

        Debug.Log("[Bridge] Hand select mode exited.");
    }

    private void Move(int delta)
    {
        int count = hand.CardCount;
        if (count <= 0) return;

        int next = selIndex + delta;

        if (wrap)
            next = (next % count + count) % count; // ���� ����
        else
            next = Mathf.Clamp(next, 0, count - 1);

        if (next != selIndex)
        {
            selIndex = next;
            SnapSelectorTo(selIndex);
        }
    }

    private void SnapSelectorTo(int index)
    {
        var rt = hand.GetCardRect(index);
        if (!rt || !selector) return;

        // ȭ�� ��ǥ ���� ����(���� Canvas�� ������ position���� OK)
        selector.rectTransform.position = rt.position;

        // ���� �ڽ� ũ�⸦ ī�忡 �����ְ� �ʹٸ�(����):
        // selector.rectTransform.sizeDelta = rt.sizeDelta;
    }
}
