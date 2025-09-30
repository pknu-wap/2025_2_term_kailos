using UnityEngine;

public class SaveButtonHandler : MonoBehaviour
{
    public void OnClickSave()
    {
        if (CardStateRuntime.Instance != null)
        {
            CardStateRuntime.Instance.SaveNow();
        }
        else
        {
            Debug.LogWarning("[SaveButtonHandler] CardStateRuntime.Instance °¡ ¾øÀ½");
        }
    }
}
