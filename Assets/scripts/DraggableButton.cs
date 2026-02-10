using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableButton : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Default Settings")]
    public Vector2 defaultPosition;
    public Vector2 defaultSize;
    
    [Header("Edit Mode")]
    public bool isEditMode = false;
    public GameObject resizeHandle;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private Button buttonComponent;
    private Vector2 originalPosition;
    private bool isDragging = false;
    
    void Awake()
    {
        EnsureInitialized();
    }

    private bool isInitialized = false;
    public void EnsureInitialized()
    {
        if (isInitialized) return;
        
        InitializeComponents();
        
        if (resizeHandle == null)
        {
            CreateResizeHandle();
        }
        
        LoadCustomSettings();
        isInitialized = true;
    }

    private void InitializeComponents()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (buttonComponent == null) buttonComponent = GetComponent<Button>();
        
        // Save initial position and size as defaults if not already set
        if (rectTransform != null)
        {
            if (defaultPosition == Vector2.zero)
            {
                defaultPosition = rectTransform.anchoredPosition;
            }
            if (defaultSize == Vector2.zero)
            {
                defaultSize = rectTransform.sizeDelta;
            }
        }
    }
    
    void Start()
    {
        EnsureInitialized();
        Debug.Log($"DraggableButton on {gameObject.name}: Start() ready. isEditMode={isEditMode}");
    }
    
    void CreateResizeHandle()
    {
        GameObject handleObj = new GameObject("ResizeHandle");
        handleObj.transform.SetParent(transform, false);
        
        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = new Color(1, 1, 0, 0.5f); // Yellow semi-transparent
        
        RectTransform handleRect = handleObj.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(1, 0); // Bottom-right corner
        handleRect.anchorMax = new Vector2(1, 0);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(40, 40); // Slightly larger
        handleRect.anchoredPosition = Vector2.zero;
        
        // Ensure handle is on top of button content
        handleObj.transform.SetAsLastSibling();
        
        // Add resize drag handler
        ResizeHandler resizer = handleObj.AddComponent<ResizeHandler>();
        resizer.targetButton = this;
        
        resizeHandle = handleObj;
        resizeHandle.SetActive(false);
    }
    
    public void SetEditMode(bool enabled)
    {
        EnsureInitialized();
        Debug.Log($"DraggableButton on {gameObject.name}: SetEditMode({enabled}) called");
        isEditMode = enabled;
        
        if (rectTransform != null)
        {
            // Removed forced scale normalization as it changes button sizes unexpectedly.
            // Only warn if scale is zero which would make it invisible/unclickable.
            if (enabled && (rectTransform.localScale.x == 0 || rectTransform.localScale.y == 0))
            {
                Debug.LogWarning($"DraggableButton on {gameObject.name}: Scale is ZERO! Resetting to 1,1,1 for visibility.");
                rectTransform.localScale = Vector3.one;
            }
        }

        // 1. First, make EVERY Graphic in children NOT block raycasts
        // This prevents "Text (TMP)" or other child images from stealing clicks
        Graphic[] allGraphics = GetComponentsInChildren<Graphic>(true);
        foreach (Graphic g in allGraphics)
        {
            // Only the ResizeHandle and the ROOT should keep raycasting
            if (g.gameObject == gameObject || g.gameObject.name == "ResizeHandle")
            {
                g.raycastTarget = true;
                
                // Visual tint on the main button
                if (enabled && g.gameObject == gameObject && g is Image img)
                {
                    img.color = new Color(1f, 1.0f, 0.7f, 1f);
                }
                else if (!enabled && g.gameObject == gameObject)
                {
                    g.color = Color.white;
                }
            }
            else
            {
                // Child texts/images should be transparent to raycasts
                g.raycastTarget = false;
            }
        }

        // 2. Ensure root has a Graphic that can catch hits
        if (GetComponent<Graphic>() == null)
        {
            Debug.LogWarning($"DraggableButton on {gameObject.name}: No Graphic on root! Adding a transparent Image.");
            Image rootImg = gameObject.AddComponent<Image>();
            rootImg.color = new Color(1, 1, 1, 0.01f); // Micro-opaque
            rootImg.raycastTarget = true;
        }
        else
        {
            GetComponent<Graphic>().raycastTarget = true;
        }

        if (buttonComponent != null)
        {
            buttonComponent.enabled = !enabled;
        }

        if (enabled)
        {
            CheckInteractivityChain();
        }
        
        if (resizeHandle != null)
        {
            resizeHandle.SetActive(enabled);
            if (enabled) 
            {
                resizeHandle.transform.SetAsLastSibling();
                Image handleImg = resizeHandle.GetComponent<Image>();
                if (handleImg != null)
                {
                    handleImg.color = Color.yellow;
                    handleImg.raycastTarget = true;
                }
            }
        }
    }

    private void CheckInteractivityChain()
    {
        Transform current = transform.parent;
        Debug.Log($"--- Parent Chain Check for {gameObject.name} ---");
        while (current != null)
        {
            CanvasGroup cg = current.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                Debug.Log($"Parent: {current.name} | CanvasGroup: [interactable={cg.interactable}, blocksRaycasts={cg.blocksRaycasts}]");
                if (!cg.interactable || !cg.blocksRaycasts)
                {
                    Debug.LogError($"CRITICAL: Parent '{current.name}' is blocking interaction for {gameObject.name}!");
                }
            }
            current = current.parent;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isEditMode)
        {
            Debug.Log($"DraggableButton on {gameObject.name}: POINTER DOWN detected! (Pos: {eventData.position})");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isEditMode)
        {
            Debug.Log($"DraggableButton on {gameObject.name}: HOVER ENTER");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isEditMode)
        {
            Debug.Log($"DraggableButton on {gameObject.name}: HOVER EXIT");
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isEditMode) return;
        
        isDragging = true;
        originalPosition = rectTransform.anchoredPosition;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isEditMode || !isDragging) return;
        
        // Move the button
        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition);
        
        rectTransform.anchoredPosition = localPointerPosition;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isEditMode) return;
        
        isDragging = false;
        SaveCustomSettings();
    }
    
    public void ResizeButton(Vector2 delta)
    {
        // Adjust delta by canvas scale factor
        if (canvas != null)
        {
            delta /= canvas.scaleFactor;
        }

        Vector2 newSize = rectTransform.sizeDelta + new Vector2(delta.x, -delta.y); // Y is inverted in UI
        
        // Clamp size to reasonable limits (expanded for flexibility)
        newSize.x = Mathf.Clamp(newSize.x, 50, 600);
        newSize.y = Mathf.Clamp(newSize.y, 50, 600);
        
        rectTransform.sizeDelta = newSize;
        SaveCustomSettings();
    }
    
    public void RestoreDefault()
    {
        rectTransform.anchoredPosition = defaultPosition;
        rectTransform.sizeDelta = defaultSize;
        SaveCustomSettings();
    }
    
    public void SaveCustomSettings()
    {
        string buttonName = gameObject.name;
        PlayerPrefs.SetFloat($"Button_{buttonName}_X", rectTransform.anchoredPosition.x);
        PlayerPrefs.SetFloat($"Button_{buttonName}_Y", rectTransform.anchoredPosition.y);
        PlayerPrefs.SetFloat($"Button_{buttonName}_Width", rectTransform.sizeDelta.x);
        PlayerPrefs.SetFloat($"Button_{buttonName}_Height", rectTransform.sizeDelta.y);
        PlayerPrefs.Save();
        
        Debug.Log($"Saved settings for {buttonName} (Pos: {rectTransform.anchoredPosition}, Size: {rectTransform.sizeDelta})");
    }
    
    public void LoadCustomSettings()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        string buttonName = gameObject.name;
        
        if (PlayerPrefs.HasKey($"Button_{buttonName}_X"))
        {
            float x = PlayerPrefs.GetFloat($"Button_{buttonName}_X");
            float y = PlayerPrefs.GetFloat($"Button_{buttonName}_Y");
            float width = PlayerPrefs.GetFloat($"Button_{buttonName}_Width");
            float height = PlayerPrefs.GetFloat($"Button_{buttonName}_Height");
            
            rectTransform.anchoredPosition = new Vector2(x, y);
            rectTransform.sizeDelta = new Vector2(width, height);
            
            Debug.Log($"Loaded custom settings for {buttonName}");
        }
    }
}

// Helper class for resize handle dragging
public class ResizeHandler : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public DraggableButton targetButton;
    private Vector2 startSize;
    private Vector2 startMousePosition;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (targetButton == null) return;
        
        startSize = targetButton.GetComponent<RectTransform>().sizeDelta;
        startMousePosition = eventData.position;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (targetButton == null) return;
        
        // Use delta instead of absolute position comparison for smoother scaling
        targetButton.ResizeButton(eventData.delta);
    }
}
