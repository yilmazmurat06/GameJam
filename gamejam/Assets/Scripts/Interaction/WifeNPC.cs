using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The Wife NPC - key story character in the psychological thriller.
/// Triggers mask reveal cutscene and eventually leaves to dungeon.
/// </summary>
public class WifeNPC : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string[] _dialoguePrompts = new string[]
    {
        "Press E to talk",
        "Press E to talk again",
        "Press E..."
    };
    
    [Header("State")]
    [SerializeField] private int _interactionCount = 0;
    [SerializeField] private bool _hasRevealedMask = false;
    [SerializeField] private bool _hasLeft = false;
    
    [Header("Events")]
    [SerializeField] private UnityEvent OnFirstInteraction;
    [SerializeField] private UnityEvent OnMaskReveal;
    [SerializeField] private UnityEvent OnLeaveRoom;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _maskSprite;
    
    // IInteractable implementation
    public bool CanInteract => !_hasLeft;
    
    public string InteractionPrompt
    {
        get
        {
            if (_hasLeft) return "";
            int index = Mathf.Min(_interactionCount, _dialoguePrompts.Length - 1);
            return _dialoguePrompts[index];
        }
    }
    
    public void OnInteract(PlayerController player)
    {
        if (_hasLeft) return;
        
        _interactionCount++;
        
        switch (_interactionCount)
        {
            case 1:
                // First interaction - normal dialogue
                HandleFirstInteraction();
                break;
            
            case 2:
                // Second interaction - mask reveal!
                HandleMaskReveal();
                break;
            
            case 3:
            default:
                // Third+ interaction - wife leaves
                HandleWifeLeaves();
                break;
        }
    }
    
    private void HandleFirstInteraction()
    {
        Debug.Log("[WifeNPC] First interaction - dialogue");
        
        // Trigger cutscene
        GameEvents.TriggerCutsceneStart();
        
        // Play dialogue/animation
        OnFirstInteraction?.Invoke();
        
        // End cutscene after delay (in real game, this would be after dialogue)
        Invoke(nameof(EndCurrentCutscene), 2f);
    }
    
    private void HandleMaskReveal()
    {
        Debug.Log("[WifeNPC] MASK REVEAL!");
        _hasRevealedMask = true;
        
        // Trigger cutscene
        GameEvents.TriggerCutsceneStart();
        
        // Swap to mask sprite
        if (_spriteRenderer != null && _maskSprite != null)
        {
            _spriteRenderer.sprite = _maskSprite;
        }
        
        // Fire global event
        GameEvents.TriggerWifeMaskRevealed();
        
        // Invoke Unity event
        OnMaskReveal?.Invoke();
        
        // End cutscene
        Invoke(nameof(EndCurrentCutscene), 3f);
    }
    
    private void HandleWifeLeaves()
    {
        Debug.Log("[WifeNPC] Wife leaves to dungeon...");
        _hasLeft = true;
        
        // Trigger cutscene
        GameEvents.TriggerCutsceneStart();
        
        // Fire events
        GameEvents.TriggerWifeLeftRoom();
        OnLeaveRoom?.Invoke();
        
        // Move/disappear (simple version - just disable)
        // In real game, animate walking away
        Invoke(nameof(DisableWife), 1.5f);
        Invoke(nameof(EndCurrentCutscene), 2f);
    }
    
    private void DisableWife()
    {
        gameObject.SetActive(false);
    }
    
    private void EndCurrentCutscene()
    {
        GameEvents.TriggerCutsceneEnd();
    }
    
    // Reset for testing
    [ContextMenu("Reset NPC State")]
    public void ResetState()
    {
        _interactionCount = 0;
        _hasRevealedMask = false;
        _hasLeft = false;
        
        if (_spriteRenderer != null && _normalSprite != null)
        {
            _spriteRenderer.sprite = _normalSprite;
        }
    }
}