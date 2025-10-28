// BattleNextScene.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleNextScene : MonoBehaviour
{
    [SerializeField] private string battleSceneName = "battle";

    // ��ư�� OnClick�� �ٷ� �����ؼ� ���ڿ� ���ڸ� �ѱ� �� ���� (��: "Enemy1")
    public void GoBattleWithEnemy(string enemyName)
    {
        // ���ؽ�Ʈ ������ �Ｎ ����
        if (SceneLoadContext.Instance == null)
        {
            var go = new GameObject("SceneLoadContext");
            go.AddComponent<SceneLoadContext>();
        }

        SceneLoadContext.Instance.pendingEnemyName = string.IsNullOrEmpty(enemyName) ? "Enemy1" : enemyName;

        // �� ��ȯ
        Time.timeScale = 1f;
        SceneManager.LoadScene(battleSceneName, LoadSceneMode.Single);
    }
}
