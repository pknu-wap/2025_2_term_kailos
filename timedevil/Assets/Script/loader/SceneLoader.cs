using UnityEngine;
using UnityEngine.SceneManagement;

/// 모든 씬 이동의 공통 유틸: 현재 씬/좌표 저장 + 씬 전환
public static class SceneLoader
{
    /// 메인씬 등에서 다른 씬으로 넘어갈 때, 플레이어 복귀용 정보 저장
    public static void SaveReturnPoint(Transform playerT, Transform triggerT = null)
    {
        PlayerReturnContext.ReturnSceneName = SceneManager.GetActiveScene().name;
        PlayerReturnContext.HasReturnPosition = true;
        PlayerReturnContext.ReturnPosition = playerT ? (Vector2)playerT.position : Vector2.zero;

        // (옵션) 트리거 오브젝트 정보
        PlayerReturnContext.MonsterReturnPosition = triggerT ? (Vector2)triggerT.position : Vector2.zero;
        PlayerReturnContext.MonsterNameInScene = triggerT ? triggerT.gameObject.name : "";
    }

    public static void Load(string sceneName, bool useFaderIfExists = true)
    {
        if (useFaderIfExists && SceneFader.instance != null)
            SceneFader.instance.LoadSceneWithFade(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}
