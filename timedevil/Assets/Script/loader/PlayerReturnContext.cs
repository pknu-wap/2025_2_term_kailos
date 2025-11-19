using UnityEngine;

public static class PlayerReturnContext
{
    public static string ReturnSceneName;
    public static bool HasReturnPosition;
    public static Vector2 ReturnPosition;

    public static Vector2 MonsterReturnPosition;
    public static string MonsterNameInScene;

    public static bool IsInGracePeriod = false; // 충돌 무적시간 등 사용 시
}
