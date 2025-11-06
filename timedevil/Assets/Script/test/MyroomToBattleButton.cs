// MyroomToBattleButton.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyroomToBattleButton : MonoBehaviour
{
    [SerializeField] string battleSceneName = "battle"; // 배틀 씬 이름
    [SerializeField] string enemyName = "Enemy1";       // 전달할 적 이름

    // ★ 버튼 OnClick에서 호출할 메서드: 반드시 public void, 매개변수 없음
    public void GoToBattle()
    {
        BattleArgs.enemyName = string.IsNullOrEmpty(enemyName) ? "Enemy1" : enemyName;
        SceneManager.LoadScene(battleSceneName);
    }
}
