using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadMyRoom()
    {
        Debug.Log("��ư�� ���Ƚ��ϴ�. Myroom ���� �ε� �õ��մϴ�.");

        try
        {
            SceneManager.LoadScene("Myroom");
            Debug.Log("�� �ε� ȣ�� �Ϸ�.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("�� �ε� �� ���� �߻�: " + e.Message);
        }
    }
}

