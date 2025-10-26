// SaveButtonHandler.cs
using UnityEngine;

public class SaveButtonHandler : MonoBehaviour
{
    // ī�常 ����
    public void SaveCardsOnly()
    {
        if (CardStateRuntime.Instance != null)
        {
            CardStateRuntime.Instance.SaveNow(); // ���ο��� card_state.json ������ ����
            Debug.Log("[SaveButtonHandler] Saved Cards");
        }
        else
        {
            Debug.LogWarning("[SaveButtonHandler] CardStateRuntime.Instance is null");
        }
    }

    // �÷��̾ ����
    public void SavePlayerOnly()
    {
        if (PlayerDataRuntime.Instance != null)
        {
            PlayerDataRuntime.Instance.SaveNow(); // ���ο��� player_data.json ������ ����
            Debug.Log("[SaveButtonHandler] Saved PlayerData");
        }
        else
        {
            Debug.LogWarning("[SaveButtonHandler] PlayerDataRuntime.Instance is null");
        }
    }

    // �� �� ����
    public void SaveAll()
    {
        SaveCardsOnly();
        SavePlayerOnly();
        Debug.Log("[SaveButtonHandler] Saved Cards + PlayerData");
    }
}
