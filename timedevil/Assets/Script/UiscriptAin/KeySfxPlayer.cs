using UnityEngine;
using UnityEngine.SceneManagement;

public class KeySfxPlayer : MonoBehaviour
{
    [Header("효과음 재생용 오디오 소스")]
    [SerializeField] private AudioSource audioSource;

    [Header("효과음 클립들")]
    [SerializeField] private AudioClip arrowClip;   // ↑↓←→ 공통
    [SerializeField] private AudioClip fClip;       // F 키
    [SerializeField] private AudioClip dClip;       // D 키

    [Header("입력을 받을 인벤토리 씬 이름")]
    [SerializeField] private string inventorySceneName = "InventoryScene";
    // ↑ 실제 인벤토리 씬 이름으로 바꿔줘

    private void Awake()
    {
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

        // 이 오브젝트는 씬이 바뀌어도 파괴되지 않음
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // 지금 씬이 인벤토리 씬이 아니면 입력 안 받게
        if (SceneManager.GetActiveScene().name != inventorySceneName)
            return;

        // ↑ ↓ ← → : 같은 소리
        if (Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow))
        {
            PlayOneShotSafe(arrowClip);
        }

        // F 키
        if (Input.GetKeyDown(KeyCode.F))
        {
            PlayOneShotSafe(fClip);
        }

        // D 키
        if (Input.GetKeyDown(KeyCode.D))
        {
            PlayOneShotSafe(dClip);
        }
    }

    private void PlayOneShotSafe(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}
