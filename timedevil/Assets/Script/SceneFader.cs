using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class SceneFader : MonoBehaviour
{
    public static SceneFader instance;

    [Tooltip("페이드 효과에 사용할 UI Image")]
    public CanvasGroup fadeCanvasGroup;

    [Tooltip("페이드 인/아웃 속도")]
    public float fadeDuration = 1f;

    public static event Action OnFadeInComplete;

    private bool isFading = false; // 씬 로딩 중복 방지 플래그

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // ▼▼▼ (핵심) 씬이 로드될 때마다 OnSceneLoaded 함수를 실행하도록 등록 ▼▼▼
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// SceneManager가 씬을 성공적으로 로드했을 때마다 이 함수를 호출합니다.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새로 로드된 씬에서 '페이드 인'을 실행
        StartCoroutine(FadeInOnLoad());
    }

    /// <summary>
    /// 씬이 로드된 후 1프레임 기다렸다가 페이드 인을 실행합니다.
    /// (EventSystem이 초기화될 시간을 줍니다)
    /// </summary>
    private IEnumerator FadeInOnLoad()
    {
        // 씬의 모든 오브젝트가 Start()를 실행할 수 있도록 1프레임 대기
        yield return null;

        // 그 다음 페이드 인 실행
        yield return StartCoroutine(Fade(0f));
    }


    // 다른 스크립트에서 호출할 씬 전환 함수
    public void LoadSceneWithFade(string sceneName)
    {
        // 중복 호출 방지
        if (isFading) return;

        // instance에서 코루틴을 시작해야 씬이 바뀌어도 살아남음
        instance.StartCoroutine(FadeAndLoadScene(sceneName));
    }

    // (수정) 페이드 아웃 -> 씬 로드까지만 담당
    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        isFading = true; // 씬 전환 시작

        // 1. 페이드 아웃 실행
        yield return StartCoroutine(Fade(1f)); // 완전히 불투명하게

        // 2. 씬 로드
        SceneManager.LoadScene(sceneName);

        // 3. (제거) 페이드 인은 OnSceneLoaded가 알아서 처리함
    }

    // 실제 페이드 효과를 주는 코루틴
    public IEnumerator Fade(float targetAlpha)
    {
        // targetAlpha가 1(아웃)일 때만 isFading을 true로 설정 (씬 로드 중)
        isFading = (targetAlpha == 1f);

        float currentTime = 0f;
        float startAlpha = fadeCanvasGroup.alpha;

        fadeCanvasGroup.blocksRaycasts = true;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, currentTime / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0f)
        {
            fadeCanvasGroup.blocksRaycasts = false;
            isFading = false; // (수정) 페이드 인 완료 시 Fading 해제
            OnFadeInComplete?.Invoke();
        }
    }
}