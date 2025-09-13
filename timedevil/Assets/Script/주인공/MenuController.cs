using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject menuUI;
    public GameManager manager;

    public TextMeshProUGUI[] menuItems; // 메뉴 항목들
    public TextMeshProUGUI panelText;   // 설명 텍스트

    int currentIndex = 0;
    bool isPaused = false;

    void OnEnable()
    {
        HighlightCurrent();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }

        if (!menuUI.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = menuItems.Length - 1;
            HighlightCurrent();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentIndex++;
            if (currentIndex >= menuItems.Length) currentIndex = 0;
            HighlightCurrent();
        }

        // 🔷 E키로 실행
        if (Input.GetKeyDown(KeyCode.E))
        {
            switch (currentIndex)
            {
                case 0: // Inventory
                    Debug.Log("Inventory selected → load Inventory scene");
                    SceneManager.LoadScene("InventoryScene");
                    break;
                case 1: // Card
                    Debug.Log("Card selected");
                    SceneManager.LoadScene("Card");
                    break;
                case 2: // Option
                    Debug.Log("Option selected");
                    break;
                case 3: // Exit
                    Debug.Log("Exit selected");
                    Application.Quit();
                    break;
            }
        }
    }

    void HighlightCurrent()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (i == currentIndex)
            {
                menuItems[i].color = Color.blue;
            }
            else
            {
                menuItems[i].color = Color.white;
            }
        }

        // 🔷 설명 텍스트 출력
        switch (currentIndex)
        {
            case 0:
                panelText.text = "open inventory";
                break;
            case 1:
                panelText.text = "manage deck";
                break;
            case 2:
                panelText.text = "open option";
                break;
            case 3:
                panelText.text = "game exit";
                break;
        }
    }

    void Pause()
    {
        menuUI.SetActive(true);
        isPaused = true;
        if (manager != null) manager.isAction = true;
        Time.timeScale = 0f;
        HighlightCurrent();
    }

    void Resume()
    {
        menuUI.SetActive(false);
        isPaused = false;
        if (manager != null) manager.isAction = false;
        Time.timeScale = 1f;
    }
}
