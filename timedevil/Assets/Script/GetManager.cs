using UnityEngine;
using TMPro;

public class GetManager : MonoBehaviour
{
    public TextMeshProUGUI talkText;
    public GameObject talkPanel;
    public bool isAction;

    private GameObject scanObject;

    public void Action(GameObject scanObj)
    {
        if (scanObj == null || talkText == null || talkPanel == null)
        {
            Debug.LogWarning("[GetManager] 필드가 비어있거나 scanObj가 null임");
            return;
        }

        Debug.Log($"[GetManager] Raycast hit: {scanObj.name} (Layer: {LayerMask.LayerToName(scanObj.layer)})");

        if (!isAction)
        {
            isAction = true;
            scanObject = scanObj;
            talkText.text = $"{scanObj.name} 아이템 획득!";

            // 아이템을 ItemDatabase에 추가
            if (ItemDatabase.Instance != null)
            {
                ItemDatabase.Instance.AddItem(scanObj.name);
            }
            else
            {
                Debug.LogError("[GetManager] ItemDatabase.Instance가 존재하지 않음!");
            }
        }
        else
        {
            isAction = false;
        }

        talkPanel.SetActive(isAction);
    }
}
