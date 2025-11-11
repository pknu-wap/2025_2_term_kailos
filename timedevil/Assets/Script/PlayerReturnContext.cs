using UnityEngine;

public static class PlayerReturnContext
{
    // (기존 변수)
    public static string ReturnSceneName;
    public static Vector3 ReturnPosition;
    public static bool HasReturnPosition = false;
    public static bool IsInGracePeriod = false;

    // ▼▼▼ [핵심 추가] 몬스터의 상태도 저장합니다 ▼▼▼
    public static Vector3 MonsterReturnPosition;
    public static string MonsterNameInScene; // (씬에서 몬스터를 찾기 위한 이름)
    // ▲▲▲
}