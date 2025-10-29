// BattleBootstrapEnsurePlayerData.cs (�� ��ũ��Ʈ, ��Ʋ ���� �� ������Ʈ�� ���̱�)
using UnityEngine;

public class BattleBootstrapEnsurePlayerData : MonoBehaviour
{
    void Awake()
    {
        if (PlayerDataRuntime.Instance == null)
        {
            var go = new GameObject("PlayerDataRuntime (Auto)");
            go.AddComponent<PlayerDataRuntime>();  // Awake���� DontDestroyOnLoad + �ε�/�⺻�� ����
            Debug.Log("[BattleBootstrap] Auto-created PlayerDataRuntime in battle scene.");
        }
    }
}
