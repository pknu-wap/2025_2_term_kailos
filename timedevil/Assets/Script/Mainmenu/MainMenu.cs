using UnityEngine;
public class MainMenu : MonoBehaviour
{
    public void LoadMyRoom()
    {
        Debug.Log("LoadMyRoom �Լ��� ȣ��Ǿ����ϴ�!");

        if (SceneFader.instance != null)
        {
            Debug.Log("SceneFader�� ã�ҽ��ϴ�. �� ��ȯ�� ��û�մϴ�.");
            SceneFader.instance.LoadSceneWithFade("Myroom");
        }
        else
        {
            Debug.LogError("SceneFader.instance�� null�Դϴ�! SceneFader ������Ʈ�� ���� ���ų� ��Ȱ��ȭ�� �� �����ϴ�.");
        }
    }
}