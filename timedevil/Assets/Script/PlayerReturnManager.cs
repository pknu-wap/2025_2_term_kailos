using UnityEngine;

/// <summary>
/// 씬이 로드될 때 'PlayerReturnContext'를 확인하여
/// 플레이어를 지정된 복귀 지점으로 이동시키는 스크립트.
/// </summary>
public class PlayerReturnManager : MonoBehaviour
{
    void Start()
    {
        // 1. 'PlayerReturnContext'에 저장된 복귀 데이터가 있는지 확인
        if (PlayerReturnContext.HasReturnPosition)
        {
            // 2. 씬에 있는 플레이어 오브젝트를 찾음
            PlayerAction player = FindObjectOfType<PlayerAction>();
            if (player != null)
            {
                // 3. 플레이어의 위치를 저장된 'ReturnPosition'으로 강제 이동
                player.transform.position = PlayerReturnContext.ReturnPosition;

                // 4. (필수) 사용한 복귀 데이터를 리셋하여,
                // 다음에 이 씬에 그냥 들어올 때 텔레포트되지 않도록 함
                PlayerReturnContext.HasReturnPosition = false;
                PlayerReturnContext.ReturnSceneName = null;
            }
            else
            {
                Debug.LogError("[PlayerReturnManager] 씬에서 PlayerAction을 찾을 수 없습니다!");
            }
        }
    }
}