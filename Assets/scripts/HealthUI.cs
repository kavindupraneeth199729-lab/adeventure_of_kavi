using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance;
    private Image playerHealthFill;
    private Canvas mainCanvas;

    private GameObject enemyHealthBarObj;
    private Image enemyHealthFill;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"HealthUI: Duplicate instance on {gameObject.name} destroyed. Keeping existing instance on {Instance.gameObject.name}");
            Destroy(this); // DESTROY ONLY THE SCRIPT, NOT THE OBJECT!
            return;
        }
        Instance = this;
        InitializeCanvas();
        CreatePlayerHealthBar();
        CreateEnemyHealthBar(); // Initialize but hide
    }

    void InitializeCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        if (canvases.Length > 0)
        {
            mainCanvas = canvases[0];
        }
        else
        {
            GameObject canvasObj = new GameObject("GameUI");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
    }

    void CreatePlayerHealthBar()
    {
        GameObject bgObj = new GameObject("PlayerHealthBar_BG");
        bgObj.transform.SetParent(mainCanvas.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.black;
        
        RectTransform bgRect = bgImage.rectTransform;
        bgRect.anchorMin = new Vector2(0, 1);
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.pivot = new Vector2(0, 1);
        bgRect.anchoredPosition = new Vector2(50, -50);
        bgRect.sizeDelta = new Vector2(300, 40);

        GameObject fillObj = new GameObject("PlayerHealthBar_Fill");
        fillObj.transform.SetParent(bgObj.transform, false);
        playerHealthFill = fillObj.AddComponent<Image>();
        playerHealthFill.color = Color.green;

        RectTransform fillRect = playerHealthFill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-10, -10);
    }
    
    void CreateEnemyHealthBar()
    {
        enemyHealthBarObj = new GameObject("EnemyHealthBar_BG");
        enemyHealthBarObj.transform.SetParent(mainCanvas.transform, false);
        Image bgImage = enemyHealthBarObj.AddComponent<Image>();
        bgImage.color = Color.black;
        
        // TOP RIGHT ALIGNMENT
        RectTransform bgRect = bgImage.rectTransform;
        bgRect.anchorMin = new Vector2(1, 1);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.pivot = new Vector2(1, 1);
        bgRect.anchoredPosition = new Vector2(-50, -50); // Padding from right
        bgRect.sizeDelta = new Vector2(300, 40);

        GameObject fillObj = new GameObject("EnemyHealthBar_Fill");
        fillObj.transform.SetParent(enemyHealthBarObj.transform, false);
        enemyHealthFill = fillObj.AddComponent<Image>();
        enemyHealthFill.color = Color.red;

        RectTransform fillRect = enemyHealthFill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-10, -10);

        // CREATE TEXT LABEL
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(enemyHealthBarObj.transform, false);
        Text lbl = textObj.AddComponent<Text>();
        lbl.text = "Dino Enemy";
        lbl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lbl.alignment = TextAnchor.MiddleCenter;
        lbl.color = Color.white;
        lbl.fontSize = 24;
        
        RectTransform txtRect = lbl.rectTransform;
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;
        
        enemyHealthBarObj.SetActive(false); // Hide initially
    }

    public void UpdatePlayerHealth(float pct)
    {
        if (playerHealthFill != null)
        {
            playerHealthFill.fillAmount = pct;
            playerHealthFill.color = Color.Lerp(Color.red, Color.green, pct);
        }
    }
    
    public void UpdateEnemyHealth(float pct)
    {
        if (enemyHealthBarObj != null) enemyHealthBarObj.SetActive(true); // Show on update
        
        if (enemyHealthFill != null)
        {
            enemyHealthFill.fillAmount = pct;
            enemyHealthFill.color = Color.Lerp(Color.red, Color.green, pct);
        }
    }
    
    public void SetEnemyName(string name)
    {
        if (enemyHealthBarObj != null)
        {
            Text lbl = enemyHealthBarObj.GetComponentInChildren<Text>();
            if (lbl != null) lbl.text = name;
        }
    }

    public void HideEnemyHealth()
    {
        if (enemyHealthBarObj != null) enemyHealthBarObj.SetActive(false);
    }
}
