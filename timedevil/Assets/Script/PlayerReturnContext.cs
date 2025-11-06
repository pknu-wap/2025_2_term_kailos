using UnityEngine;

/// <summary>
/// 씬(Scene) 간에 플레이어의 '복귀 정보'를 전달하기 위한 정적(static) 클래스.
/// (이 스크립트는 씬 오브젝트에 붙이지 않습니다)
/// </summary>
public static class PlayerReturnContext
{
    // 1. 배틀이 끝난 후 돌아올 씬의 이름 (예: "MyRoom")
    public static string ReturnSceneName;

    // 2. 돌아올 씬에서 플레이어가 배치될 정확한 좌표
    public static Vector3 ReturnPosition;

    // 3. '돌아올 좌표'가 설정되었는지 확인하는 플래그
    // (이게 false면 그냥 씬의 기본 위치에서 시작)
    public static bool HasReturnPosition = false;
}