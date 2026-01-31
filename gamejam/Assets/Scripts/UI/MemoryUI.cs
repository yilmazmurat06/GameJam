using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Full-screen overlay for memory/narrative visuals.
/// Displays a static image with optional text.
/// </summary>
public class MemoryUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _memoryImage;
    [SerializeField] private Text _memoryText;
    [SerializeField] private Text _continuePrompt;
    
    [Header("Fade Settings")]
    [SerializeField] private float _fadeInDuration = 1f;
    [SerializeField] private float _fadeOutDuration = 0.5f;
    
    private CanvasGroup _canvasGroup;
    private bool _isShowing;
    private bool _waitingForInput;
    private float _displayTimer;
    
    private void Awake()
    {
        SetupUI();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
    }
    
    private void SetupUI()
    {
        // Create background
        if (_backgroundImage == null)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform, false);
            _backgroundImage = bgObj.AddComponent<Image>();
            _backgroundImage.color = Color.black;
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
        }
        
        // Create memory image container
        if (_memoryImage == null)
        {
            GameObject imgObj = new GameObject("MemoryImage");
            imgObj.transform.SetParent(transform, false);
            _memoryImage = imgObj.AddComponent<Image>();
            _memoryImage.preserveAspect = true;
            
            RectTransform imgRect = imgObj.GetComponent<RectTransform>();
            imgRect.anchorMin = new Vector2(0.1f, 0.1f);
            imgRect.anchorMax = new Vector2(0.9f, 0.85f);
            imgRect.offsetMin = Vector2.zero;
            imgRect.offsetMax = Vector2.zero;
        }
        
        // Create text
        if (_memoryText == null)
        {
            GameObject textObj = new GameObject("MemoryText");
            textObj.transform.SetParent(transform, false);
            _memoryText = textObj.AddComponent<Text>();
            _memoryText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _memoryText.fontSize = 24;
            _memoryText.alignment = TextAnchor.MiddleCenter;
            _memoryText.color = Color.white;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.02f);
            textRect.anchorMax = new Vector2(0.9f, 0.1f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        // Create continue prompt
        if (_continuePrompt == null)
        {
            GameObject promptObj = new GameObject("ContinuePrompt");
            promptObj.transform.SetParent(transform, false);
            _continuePrompt = promptObj.AddComponent<Text>();
            _continuePrompt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _continuePrompt.fontSize = 18;
            _continuePrompt.alignment = TextAnchor.MiddleCenter;
            _continuePrompt.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            _continuePrompt.text = "Press any key to continue...";
            
            RectTransform promptRect = promptObj.GetComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0.3f, 0.01f);
            promptRect.anchorMax = new Vector2(0.7f, 0.05f);
            promptRect.offsetMin = Vector2.zero;
            promptRect.offsetMax = Vector2.zero;
        }
    }
    
    private void Update()
    {
        if (!_isShowing) return;
        
        if (_waitingForInput)
        {
            // Wait for any key
            if (Input.anyKeyDown)
            {
                Hide();
            }
        }
        else
        {
            // Timer-based
            _displayTimer -= Time.unscaledDeltaTime;
            if (_displayTimer <= 0)
            {
                Hide();
            }
        }
    }
    
    /// <summary>
    /// Show a memory image.
    /// </summary>
    public void Show(Sprite image, string text, float duration, bool waitForInput)
    {
        _memoryImage.sprite = image;
        _memoryText.text = text ?? "";
        _displayTimer = duration;
        _waitingForInput = waitForInput;
        
        _continuePrompt.enabled = waitForInput;
        
        _isShowing = true;
        StartCoroutine(FadeIn());
    }
    
    /// <summary>
    /// Hide the memory overlay.
    /// </summary>
    public void Hide()
    {
        _isShowing = false;
        StartCoroutine(FadeOut());
    }
    
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < _fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = elapsed / _fadeInDuration;
            yield return null;
        }
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }
    
    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < _fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = 1f - (elapsed / _fadeOutDuration);
            yield return null;
        }
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        
        // Resume game
        GameEvents.TriggerCutsceneEnd();
    }
}
