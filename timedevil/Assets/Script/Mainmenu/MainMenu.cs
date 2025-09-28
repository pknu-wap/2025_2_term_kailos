using UnityEngine;
public class MainMenu : MonoBehaviour
{
    public void LoadMyRoom()
    {
        Debug.Log("LoadMyRoom 함수가 호출되었습니다!");

        if (SceneFader.instance != null)
        {
            Debug.Log("SceneFader를 찾았습니다. 씬 전환을 요청합니다.");
            SceneFader.instance.LoadSceneWithFade("Myroom");
        }
        else
        {
            Debug.LogError("SceneFader.instance가 null입니다! SceneFader 오브젝트가 씬에 없거나 비활성화된 것 같습니다.");
        }
    }
}