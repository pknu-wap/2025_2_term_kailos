using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    // 이 스크립트를 어디서든 쉽게 접근할 수 있도록 만드는 싱글톤 패턴
    public static SceneFader instance;

    [Tooltip("페이드 효과에 사용할 UI Image")]
    public CanvasGroup fadeCanvasGroup; // Image 대신 CanvasGroup을 사용하면 더 유연합니다.

    [Tooltip("페이드 인/아웃 속도")]
    public float fadeDuration = 1f;

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 이 오브젝트는 파괴되지 않음
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 다른 스크립트에서 호출할 씬 전환 함수
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    // 페이드 아웃 -> 씬 로드 -> 페이드 인을 순차적으로 실행하는 코루틴
    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        // 1. 페이드 아웃 실행
        yield return StartCoroutine(Fade(1f)); // 완전히 불투명하게

        // 2. 씬 로드
        SceneManager.LoadScene(sceneName);

        // 3. 페이드 인 실행
        yield return StartCoroutine(Fade(0f)); // 완전히 투명하게
    }

    // 실제 페이드 효과를 주는 코루틴
    private IEnumerator Fade(float targetAlpha)
    {
        float currentTime = 0f;
        float startAlpha = fadeCanvasGroup.alpha;

        // CanvasGroup이 상호작용이나 레이캐스트를 막도록 설정
        fadeCanvasGroup.blocksRaycasts = true;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            // Lerp 함수를 이용해 부드럽게 투명도 조절
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, currentTime / fadeDuration);
            yield return null;
        }

        // 정확한 목표 값으로 설정
        fadeCanvasGroup.alpha = targetAlpha;

        // 페이드 인이 끝나면 상호작용을 다시 허용
        if (targetAlpha == 0f)
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }
}