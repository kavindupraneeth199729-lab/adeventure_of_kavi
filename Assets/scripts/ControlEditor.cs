using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.IO;
using System;

public class ControlEditor : MonoBehaviour
{
    public static ControlEditor Instance;
    
    [Header("UI References")]
    public GameObject editModePanel;
    public Button doneButton;
    public Button restoreDefaultButton;
    public Text instructionText;
    
    private List<DraggableButton> controlButtons = new List<DraggableButton>();
    private bool _isEditMode = false; // Internal flag for Update loop diagnostics
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize File Logging
        SetupFileLogging();
        Debug.Log("--- NEW GAME SESSION STARTED ---");
        
        // Auto-create UI if not assigned
        if (editModePanel == null)
        {
            CreateEditModeUI();
        }
        else
        {
            editModePanel.SetActive(false);
        }
    }

    private string logPath;
    void SetupFileLogging()
    {
        try 
        {
            // Write to project root
            logPath = Path.Combine(Application.dataPath, "../AdventuresOfKavi_Log.txt");
            Application.logMessageReceived += HandleLog;
            
            // Clear old log at start of session
            File.WriteAllText(logPath, "Log Session Started: " + DateTime.Now + "\n\n");
            Debug.Log($"File Logger initialized at: {logPath}");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to setup file logging: " + e.Message);
        }
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (string.IsNullOrEmpty(logPath)) return;
        
        try
        {
            string logEntry = $"[{type}] {logString}\n";
            if (type == LogType.Error || type == LogType.Exception)
            {
                logEntry += stackTrace + "\n";
            }
            File.AppendAllText(logPath, logEntry);
        }
        catch { /* Ignore logging errors to prevent loops */ }
    }
    
    void Start()
    {
        Debug.Log("ControlEditor: Start() - Auto-loading settings for all controls...");
        // Ensure buttons are found and settings loaded at game start
        FindAndSetupControlButtons();
    }
    
    void CreateEditModeUI()
    {
        // Find or create Canvas (Include inactive to be thorough)
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("ControlEditorCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 99; // Below pause menu
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create Edit Mode Panel
        GameObject panel = new GameObject("EditModePanel");
        panel.transform.SetParent(canvas.transform, false);
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.3f); // Even more transparent
        panelImage.raycastTarget = false; // CRITICAL: Don't block clicks to buttons behind!
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        panel.transform.SetAsFirstSibling(); // Send to back of canvas
        
        // Ensure the panel itself DOES NOT block raycasts to buttons BEHIND it,
        // but its OWN children (Done/Restore buttons) should still be clickable.
        // Instead of blocksRaycasts = false on the whole panel, we just ensure
        // the panel's background image doesn't block.
        panelImage.raycastTarget = false;
        
        editModePanel = panel;
        
        // Instruction Text
        GameObject instructionObj = new GameObject("InstructionText");
        instructionObj.transform.SetParent(panel.transform, false);
        
        instructionText = instructionObj.AddComponent<Text>();
        instructionText.text = "Drag buttons to move\nDrag yellow handle to resize";
        instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionText.fontSize = 24;
        instructionText.alignment = TextAnchor.MiddleCenter;
        instructionText.color = Color.white;
        
        RectTransform instructionRect = instructionObj.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0.5f, 0.85f);
        instructionRect.anchorMax = new Vector2(0.5f, 0.85f);
        instructionRect.sizeDelta = new Vector2(400, 100);
        instructionRect.anchoredPosition = Vector2.zero;
        
        // Done Button
        GameObject doneObj = CreateButton("DoneButton", "Done", new Vector2(0.5f, 0.2f), panel.transform);
        doneButton = doneObj.GetComponent<Button>();
        doneButton.onClick.AddListener(DisableEditMode);
        
        // Restore Default Button
        GameObject restoreObj = CreateButton("RestoreDefaultButton", "Restore Default", new Vector2(0.5f, 0.1f), panel.transform);
        restoreDefaultButton = restoreObj.GetComponent<Button>();
        restoreDefaultButton.onClick.AddListener(RestoreAllToDefault);
        
        // Hide panel initially
        editModePanel.SetActive(false);
    }
    
    GameObject CreateButton(string name, string buttonText, Vector2 anchorPosition, Transform parent)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.7f, 0.2f, 1f); // Green
        
        Button button = buttonObj.AddComponent<Button>();
        
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        button.colors = colors;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorPosition;
        buttonRect.anchorMax = anchorPosition;
        buttonRect.sizeDelta = new Vector2(250, 60);
        buttonRect.anchoredPosition = Vector2.zero;
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 28;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return buttonObj;
    }
    
    void FindAndSetupControlButtons()
    {
        Debug.Log("ControlEditor: FindAndSetupControlButtons() called");
        
        // Use FindObjectsByType which is newer and more reliable for scene objects
        // FindObjectsInactive.Include will find buttons even if they are currently disabled
        Button[] allButtonsInScene = GameObject.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"ControlEditor: Found {allButtonsInScene.Length} total buttons in the scene");

        string[] targetNames = new string[] 
        { 
            "Attack1", "Attack2", "Attack3",
            "Button", "Button (1)", "Button (2)",
            "MoveLeftButton", "MoveRightButton", "JumpButton", 
            "AttackButton", "Attack1Button", "Attack2Button", "Attack3Button",
            "LeftButton", "RightButton"
        };
        
        foreach (Button btn in allButtonsInScene)
        {
            // Skip UI templates or buttons in other canvases we don't want to edit
            // (Optional: filter by canvas name if needed)
            
            bool isMatch = false;
            foreach (string targetName in targetNames)
            {
                if (btn.gameObject.name == targetName)
                {
                    isMatch = true;
                    break;
                }
            }

            if (isMatch)
            {
                DraggableButton draggable = btn.GetComponent<DraggableButton>();
                if (draggable == null)
                {
                    draggable = btn.gameObject.AddComponent<DraggableButton>();
                    Debug.Log($"ControlEditor: -> Added DraggableButton to matchmaking button: {btn.gameObject.name}");
                }
                
                if (!controlButtons.Contains(draggable))
                {
                    controlButtons.Add(draggable);
                    Debug.Log($"ControlEditor: -> Registered button for editing: {btn.gameObject.name}");
                    // Initialize and load settings immediately
                    draggable.EnsureInitialized();
                }
            }
        }
        
        // Also search by tag if buttons are tagged - REMOVED TO PREVENT CRASH IF TAG NOT DEFINED
        /*
        GameObject[] taggedButtons = GameObject.FindGameObjectsWithTag("ControlButton");
        foreach (GameObject buttonObj in taggedButtons)
        {
            DraggableButton draggable = buttonObj.GetComponent<DraggableButton>();
            if (draggable == null)
            {
                draggable = buttonObj.AddComponent<DraggableButton>();
            }
            if (!controlButtons.Contains(draggable))
            {
                controlButtons.Add(draggable);
                Debug.Log($"Found tagged control button: {buttonObj.name}");
            }
        }
        */
        
        Debug.Log($"Total control buttons found: {controlButtons.Count}");
    }
    
    public void EnableEditMode()
    {
        Debug.Log("ControlEditor: EnableEditMode() called");
        
        // Re-find buttons every time to be safe
        controlButtons.Clear();
        FindAndSetupControlButtons();
        
        _isEditMode = true;
        Time.timeScale = 0f; // Keep game paused
        
        RunDiagnostic(); // Automatic diagnostic on enable
        
        if (editModePanel != null)
        {
            editModePanel.SetActive(true);
        }
        
        // Enable edit mode on all control buttons
        foreach (DraggableButton button in controlButtons)
        {
            if (button != null)
            {
                button.SetEditMode(true);
                Debug.Log($"ControlEditor: Enabled edit on {button.gameObject.name}");
                
                // Diagnostic: Print button info
                RectTransform rt = button.GetComponent<RectTransform>();
                Canvas cv = button.GetComponentInParent<Canvas>();
                Debug.Log($"ControlEditor: Diagnostic - {button.name} | Pos: {rt.anchoredPosition} | Scale: {rt.localScale} | Canvas: {(cv != null ? cv.name : "NONE")}");
            }
        }
        
        Debug.Log($"Control Edit Mode Enabled - {controlButtons.Count} buttons editable");
    }

    void Update()
    {
        if (_isEditMode && Input.GetMouseButtonDown(0))
        {
            Debug.Log($"--- ControlEditor: GLOBAL CLICK DETECTED at {Input.mousePosition} ---");
            
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            Debug.Log($"Raycast hit {results.Count} objects:");
            for (int i = 0; i < results.Count; i++)
            {
                Debug.Log($"HITS[{i}]: {results[i].gameObject.name} | Depth: {results[i].depth} | SortingLayer: {results[i].sortingLayer} | Dist: {results[i].distance}");
            }
            
            if (results.Count == 0)
            {
                Debug.LogWarning("No UI objects were hit! Is there an EventSystem? Is the Canvas blocking?");
            }
        }
    }

    [ContextMenu("Debug Diagnostic")]
    public void RunDiagnostic()
    {
        Debug.Log("--- CONTROL EDITOR DIAGNOSTIC ---");
        EventSystem es = EventSystem.current;
        Debug.Log($"EventSystem: {(es != null ? es.name : "MISSING!")}");
        if (es != null) Debug.Log($"Input Module: {es.GetComponent<BaseInputModule>()?.GetType().Name}");

        Canvas[] canvases = GameObject.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            Debug.Log($"Canvas: {c.name} | Order: {c.sortingOrder} | Mode: {c.renderMode} | Raycaster: {(c.GetComponent<GraphicRaycaster>() != null ? "YES" : "NO")}");
        }
    }
    
    public void DisableEditMode()
    {
        _isEditMode = false;
        Time.timeScale = 1f; // Resume game
        
        if (editModePanel != null)
        {
            editModePanel.SetActive(false);
        }
        
        // Disable edit mode on all control buttons and save final state
        foreach (DraggableButton button in controlButtons)
        {
            if (button != null)
            {
                button.SetEditMode(false);
                button.SaveCustomSettings();
            }
        }
        
        PlayerPrefs.Save(); // Force save to disk for Android persistence
        Debug.Log("Control Edit Mode Disabled - Final settings saved and disk flushed");
    }
    
    public void RestoreAllToDefault()
    {
        foreach (DraggableButton button in controlButtons)
        {
            if (button != null)
            {
                button.RestoreDefault();
            }
        }
        
        Debug.Log("All controls restored to default positions and sizes");
    }
}
