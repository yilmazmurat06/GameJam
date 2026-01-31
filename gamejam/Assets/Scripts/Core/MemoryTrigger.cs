using UnityEngine;

/// <summary>
/// Trigger zone that displays a full-screen memory image.
/// Place in each room to show critical narrative moment.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MemoryTrigger : MonoBehaviour
{
    [Header("Memory Settings")]
    [SerializeField] private Sprite _memoryImage;
    [SerializeField] private string _memoryText;
    [SerializeField] private float _displayDuration = 5f;
    [SerializeField] private bool _waitForInput = true;
    [SerializeField] private bool _triggerOnce = true;
    
    private bool _hasTriggered;
    
    private void Awake()
    {
        // Ensure trigger is set
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggerOnce && _hasTriggered) return;
        
        // Check if player entered
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            ShowMemory();
            _hasTriggered = true;
        }
    }
    
    private void ShowMemory()
    {
        // Find or create MemoryUI
        MemoryUI ui = FindObjectOfType<MemoryUI>();
        if (ui == null)
        {
            // Create canvas with MemoryUI
            GameObject canvas = new GameObject("MemoryCanvas");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 1000;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            ui = canvas.AddComponent<MemoryUI>();
        }
        
        ui.Show(_memoryImage, _memoryText, _displayDuration, _waitForInput);
        
        // Pause player
        GameEvents.TriggerCutsceneStart();
    }
}
