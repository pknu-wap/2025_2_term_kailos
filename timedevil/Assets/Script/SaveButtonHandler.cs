using UnityEngine;

public class SaveButtonHandler : MonoBehaviour
{
    public void OnClickSave()
    {
        if (CardStateRuntime.Instance != null)
            CardStateRuntime.Instance.SaveNow();
    }

    public void OnClickToggleTalk(GameObject target)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Action(target);
    }
}
