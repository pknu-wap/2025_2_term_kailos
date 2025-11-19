using UnityEngine;
using UnityEngine.SceneManagement;

/// 메인씬이 로드될 때 Player를 복귀 좌표에 스폰/이동
public class ReturnSpawnApplier : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    private void Start()
    {
        if (!PlayerReturnContext.HasReturnPosition) return;

        // 현재 씬이 복귀 대상이면 이동
        if (PlayerReturnContext.ReturnSceneName == SceneManager.GetActiveScene().name && playerTransform)
        {
            playerTransform.position = PlayerReturnContext.ReturnPosition;
        }

        // 한 번 적용 후 플래그를 초기화할지 여부는 기획에 맞춰 선택
        // PlayerReturnContext.HasReturnPosition = false;
    }
}
