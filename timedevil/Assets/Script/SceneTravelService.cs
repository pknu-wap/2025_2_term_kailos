using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneTravelService
{
    /// 배틀로 진입하기 직전에 필요한 모든 컨텍스트를 저장하고 씬 전환
    public static void GoToBattle(
        string battleSceneName,
        string enemyIdToLoad,
        Transform playerT,
        Transform enemyT // 트리거 오브젝트(몬스터) 트랜스폼
    )
    {
        // 1) 적 ID 전달
        if (ObjectNameRuntime.Instance != null)
            ObjectNameRuntime.Instance.SetEnemyToLoad(enemyIdToLoad);
        else
            Debug.LogError("[SceneTravelService] ObjectNameRuntime.Instance is null");

        // 2) 복귀 정보 저장
        PlayerReturnContext.ReturnSceneName = SceneManager.GetActiveScene().name;
        PlayerReturnContext.HasReturnPosition = true;
        PlayerReturnContext.ReturnPosition = playerT ? (Vector2)playerT.position : Vector2.zero;
        PlayerReturnContext.MonsterReturnPosition = enemyT ? (Vector2)enemyT.position : Vector2.zero;
        PlayerReturnContext.MonsterNameInScene = enemyT ? enemyT.gameObject.name : "";

        // 3) 씬 전환(연출은 네가 쓰는 페이더 유지)
        if (SceneFader.instance != null)
            SceneFader.instance.LoadSceneWithFade(battleSceneName);
        else
            SceneManager.LoadScene(battleSceneName);
    }
}
