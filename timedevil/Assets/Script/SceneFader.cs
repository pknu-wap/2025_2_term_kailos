using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader instance;

    public CanvasGroup canvasGroup;
    public float fadeDuration = 1f;

    private void Awake()
    {
        // 싱글톤 생성
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // 씬 로드될 때마다 자동 페이드인 실행
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 첫 씬 로드 전 SceneFader 프리팹 항상 보장
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoLoadFaderPrefab()
    {
        if (FindObjectOfType<SceneFader>() != null) return;

        SceneFader prefab = Resources.Load<SceneFader>("SceneFader");
        if (prefab == null)
        {
            Debug.LogError("Resources/SceneFader.prefab 을 찾을 수 없음");
            return;
        }

        Instantiate(prefab);
    }

    //-------------------------------------------------------------------
    // 씬 로드 후 자동 페이드 인
    //-------------------------------------------------------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        instance.StartCoroutine(instance.Fade(0f));
    }

    //-------------------------------------------------------------------
    // 페이드 함수
    //-------------------------------------------------------------------
    public IEnumerator Fade(float target)
    {
        float start = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
    }

    //-------------------------------------------------------------------
    // 씬 전환
    //-------------------------------------------------------------------
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        // 어둡게
        yield return StartCoroutine(Fade(1f));

        // 씬 로드
        SceneManager.LoadScene(sceneName);
    }
}
