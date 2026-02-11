using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;
    
    [Header("UI References")]
    public Button settingsButton;
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button editControlsButton;
    
    private bool isPaused = false;
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"PauseMenuManager: Scene Loaded - {scene.name}. Re-initializing UI.");
        // Close menu on load
        isPaused = false;
        Time.timeScale = 1f;
        
        // Re-setup UI
        CreatePauseMenuUI();
        
        // Setup listeners
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(TogglePause);
        }
    }

    void Awake()
    {
        Debug.Log("PauseMenuManager: Awake() called");
        
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("PauseMenuManager: Instance created");
        }
        else
        {
            Debug.LogWarning("PauseMenuManager: Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }
        
        // Auto-create UI if not assigned
        if (settingsButton == null || pauseMenuPanel == null)
        {
            Debug.Log("PauseMenuManager: Creating auto-generated UI");
            CreatePauseMenuUI();
        }
        else
        {
            // Hide pause menu at start
            pauseMenuPanel.SetActive(false);
        }
        
        // Setup button listeners
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(TogglePause);
            Debug.Log("PauseMenuManager: Settings button listener added");
        }
        else
        {
            Debug.LogError("PauseMenuManager: settingsButton is NULL after creation!");
        }
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);
        if (editControlsButton != null)
            editControlsButton.onClick.AddListener(OpenControlEditor);
            
        Debug.Log("PauseMenuManager: Initialization complete");
    }
    
    void CreatePauseMenuUI()
    {
        Debug.Log("CreatePauseMenuUI: Starting UI creation");
        
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("PauseMenuCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // CRITICAL: Set very high sorting order to ensure it's on top
            canvas.sortingOrder = 999;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("CreatePauseMenuUI: Created new Canvas");
        }
        else
        {
            Debug.Log($"CreatePauseMenuUI: Using existing Canvas: {canvas.name}");
        }
        
        // Create Settings Button (Top-Right)
        GameObject settingsObj = new GameObject("SettingsButton");
        settingsObj.transform.SetParent(canvas.transform, false);
        
        Image settingsImage = settingsObj.AddComponent<Image>();
        settingsImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Dark gray, more opaque
        
        settingsButton = settingsObj.AddComponent<Button>();
        settingsButton.onClick.AddListener(TogglePause);
        
        RectTransform settingsRect = settingsObj.GetComponent<RectTransform>();
        settingsRect.anchorMin = new Vector2(1, 1); // Top-right
        settingsRect.anchorMax = new Vector2(1, 1);
        settingsRect.pivot = new Vector2(1, 1);
        settingsRect.sizeDelta = new Vector2(60, 60);
        settingsRect.anchoredPosition = new Vector2(-10, -10); // 10px from top-right
        
        // Settings button text (using "MENU" instead of gear icon for compatibility)
        GameObject settingsTextObj = new GameObject("Text");
        settingsTextObj.transform.SetParent(settingsObj.transform, false);
        
        Text settingsText = settingsTextObj.AddComponent<Text>();
        settingsText.text = "|||";
        settingsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        settingsText.fontSize = 30;
        settingsText.fontStyle = FontStyle.Bold;
        settingsText.alignment = TextAnchor.MiddleCenter;
        settingsText.color = Color.white;
        
        RectTransform settingsTextRect = settingsTextObj.GetComponent<RectTransform>();
        settingsTextRect.anchorMin = Vector2.zero;
        settingsTextRect.anchorMax = Vector2.one;
        settingsTextRect.offsetMin = Vector2.zero;
        settingsTextRect.offsetMax = Vector2.zero;
        
        // Create Pause Menu Panel - MAKE IT SUPER VISIBLE
        GameObject panel = new GameObject("PauseMenuPanel");
        panel.transform.SetParent(canvas.transform, false);
        panel.transform.SetAsLastSibling(); // Ensure it's on top of all other UI
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.95f); // Almost fully opaque black
        panelImage.raycastTarget = true;
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Ensure buttons are hooked up
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(Resume);
        }
        if (editControlsButton != null)
        {
            editControlsButton.onClick.RemoveAllListeners();
            editControlsButton.onClick.AddListener(OpenControlEditor);
        }

        pauseMenuPanel = panel;
        
        Debug.Log("CreatePauseMenuUI: Created pause panel");
        
        // "PAUSED" Title
        GameObject titleObj = new GameObject("PausedTitle");
        titleObj.transform.SetParent(panel.transform, false);
        
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "PAUSED";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 60;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(400, 100);
        titleRect.anchoredPosition = Vector2.zero;
        
        // Resume Button
        GameObject resumeObj = CreateMenuButton("ResumeButton", "Resume", new Vector2(0.5f, 0.5f), panel.transform);
        resumeButton = resumeObj.GetComponent<Button>();
        resumeButton.onClick.AddListener(Resume);
        
        // Edit Controls Button
        GameObject editObj = CreateMenuButton("EditControlsButton", "Edit Controls", new Vector2(0.5f, 0.35f), panel.transform);
        editControlsButton = editObj.GetComponent<Button>();
        editControlsButton.onClick.AddListener(OpenControlEditor);
        
        // Hide panel initially
        pauseMenuPanel.SetActive(false);
    }
    
    GameObject CreateMenuButton(string name, string buttonText, Vector2 anchorPosition, Transform parent)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.5f, 0.8f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Add color tinting on hover
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.6f, 0.9f, 1f);
        button.colors = colors;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorPosition;
        buttonRect.anchorMax = anchorPosition;
        buttonRect.sizeDelta = new Vector2(250, 70);
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
    
    private float lastToggleTime = 0f;
    private const float toggleCooldown = 0.5f; // Half second cooldown
    
    public void TogglePause()
    {
        // Prevent rapid toggling
        if (Time.unscaledTime - lastToggleTime < toggleCooldown)
        {
            Debug.Log("PauseMenuManager: Toggle ignored (cooldown)");
            return;
        }
        
        lastToggleTime = Time.unscaledTime;
        Debug.Log($"PauseMenuManager: TogglePause() called. isPaused={isPaused}");
        
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }
    
    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f; // Pause game
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            Debug.Log("PauseMenuManager: Pause menu activated");
        }
        else
        {
            Debug.LogError("PauseMenuManager: pauseMenuPanel is NULL!");
        }
        
        Debug.Log("Game Paused");
    }
    
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume game
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        Debug.Log("Game Resumed");
    }
    
    void OpenControlEditor()
    {
        Debug.Log("Opening Control Editor");
        
        // Close pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        // Open control editor
        if (ControlEditor.Instance != null)
        {
            ControlEditor.Instance.EnableEditMode();
        }
        else
        {
            Debug.LogWarning("ControlEditor not found!");
        }
    }

}
