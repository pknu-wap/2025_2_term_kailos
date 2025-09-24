using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    // �� ��ũ��Ʈ�� ��𼭵� ���� ������ �� �ֵ��� ����� �̱��� ����
    public static SceneFader instance;

    [Tooltip("���̵� ȿ���� ����� UI Image")]
    public CanvasGroup fadeCanvasGroup; // Image ��� CanvasGroup�� ����ϸ� �� �����մϴ�.

    [Tooltip("���̵� ��/�ƿ� �ӵ�")]
    public float fadeDuration = 1f;

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ���� �ٲ� �� ������Ʈ�� �ı����� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // �ٸ� ��ũ��Ʈ���� ȣ���� �� ��ȯ �Լ�
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    // ���̵� �ƿ� -> �� �ε� -> ���̵� ���� ���������� �����ϴ� �ڷ�ƾ
    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        // 1. ���̵� �ƿ� ����
        yield return StartCoroutine(Fade(1f)); // ������ �������ϰ�

        // 2. �� �ε�
        SceneManager.LoadScene(sceneName);

        // 3. ���̵� �� ����
        yield return StartCoroutine(Fade(0f)); // ������ �����ϰ�
    }

    // ���� ���̵� ȿ���� �ִ� �ڷ�ƾ
    private IEnumerator Fade(float targetAlpha)
    {
        float currentTime = 0f;
        float startAlpha = fadeCanvasGroup.alpha;

        // CanvasGroup�� ��ȣ�ۿ��̳� ����ĳ��Ʈ�� ������ ����
        fadeCanvasGroup.blocksRaycasts = true;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            // Lerp �Լ��� �̿��� �ε巴�� ���� ����
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, currentTime / fadeDuration);
            yield return null;
        }

        // ��Ȯ�� ��ǥ ������ ����
        fadeCanvasGroup.alpha = targetAlpha;

        // ���̵� ���� ������ ��ȣ�ۿ��� �ٽ� ���
        if (targetAlpha == 0f)
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }
}