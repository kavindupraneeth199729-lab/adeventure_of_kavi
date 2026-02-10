using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;
    
    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Button restartButton;
    public Button quitButton;
    
    private bool isGameOver = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Create UI if it doesn't exist
        if (gameOverPanel == null)
        {
            CreateGameOverUI();
        }
        else
        {
            // Hide panel at start
            gameOverPanel.SetActive(false);
        }
    }
    
    void CreateGameOverUI()
    {
        // Create Canvas if needed
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("GameOverCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Create Panel
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(canvas.transform, false);
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        gameOverPanel = panel;
        
        // Create "GAME OVER" Text
        GameObject textObj = new GameObject("GameOverText");
        textObj.transform.SetParent(panel.transform, false);
        
        Text text = textObj.AddComponent<Text>();
        text.text = "GAME OVER";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 60;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.red;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.7f);
        textRect.anchorMax = new Vector2(0.5f, 0.7f);
        textRect.sizeDelta = new Vector2(500, 100);
        textRect.anchoredPosition = Vector2.zero;
        
        gameOverText = text;
        
        // Create Restart Button
        GameObject restartObj = CreateButton("RestartButton", "Restart", new Vector2(0.5f, 0.4f), panel.transform);
        restartButton = restartObj.GetComponent<Button>();
        restartButton.onClick.AddListener(RestartGame);
        
        // Create Quit Button  
        GameObject quitObj = CreateButton("QuitButton", "Quit", new Vector2(0.5f, 0.25f), panel.transform);
        quitButton = quitObj.GetComponent<Button>();
        quitButton.onClick.AddListener(QuitGame);
        
        // Hide panel initially
        gameOverPanel.SetActive(false);
    }
    
    GameObject CreateButton(string name, string buttonText, Vector2 anchorPosition, Transform parent)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorPosition;
        buttonRect.anchorMax = anchorPosition;
        buttonRect.sizeDelta = new Vector2(200, 60);
        buttonRect.anchoredPosition = Vector2.zero;
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 30;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return buttonObj;
    }
    
    public void ShowGameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Debug.Log("GameOverManager: Showing Game Over screen");
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f; // Pause game
        }
    }
    
    void RestartGame()
    {
        Time.timeScale = 1f; // Resume time
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    void QuitGame()
    {
        Time.timeScale = 1f; // Resume time
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
