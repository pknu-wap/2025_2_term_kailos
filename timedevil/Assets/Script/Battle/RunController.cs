// RunController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunController : MonoBehaviour
{
    [SerializeField] private Button runButton;     // (옵션) Run 버튼 연결
    [SerializeField] private string myroomScene = "Myroom";

    void Awake()
    {
        if (runButton != null)
        {
            runButton.onClick.RemoveAllListeners();
            runButton.onClick.AddListener(OnRunClicked);
        }
    }

    // UI 이벤트에 직접 연결해도 됩니다.
    public void OnRunClicked()
    {
        Time.timeScale = 1f; // 혹시 멈춰있다면 복구
        SceneManager.LoadScene(myroomScene, LoadSceneMode.Single);
    }
}
