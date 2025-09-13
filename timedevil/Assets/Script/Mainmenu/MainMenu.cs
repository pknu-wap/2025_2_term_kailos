using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadMyRoom()
    {
        Debug.Log("버튼이 눌렸습니다. Myroom 씬을 로드 시도합니다.");

        try
        {
            SceneManager.LoadScene("Myroom");
            Debug.Log("씬 로드 호출 완료.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("씬 로드 중 오류 발생: " + e.Message);
        }
    }
}

