using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Generic interactable for simple objects (doors, items, etc).
/// Fires a UnityEvent when interacted with.
/// </summary>
public class GenericInteractable : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private string _interactionPrompt = "Press E to interact";
    [SerializeField] private bool _canInteract = true;
    [SerializeField] private bool _oneTimeOnly = false;
    
    [Header("Events")]
    [SerializeField] private UnityEvent OnInteracted;
    [SerializeField] private UnityEvent<PlayerController> OnInteractedWithPlayer;
    
    private bool _hasBeenUsed = false;
    
    // IInteractable implementation
    public bool CanInteract => _canInteract && !(_oneTimeOnly && _hasBeenUsed);
    public string InteractionPrompt => _interactionPrompt;
    
    public void OnInteract(PlayerController player)
    {
        if (!CanInteract) return;
        
        Debug.Log($"[GenericInteractable] {gameObject.name} interacted");
        
        _hasBeenUsed = true;
        
        OnInteracted?.Invoke();
        OnInteractedWithPlayer?.Invoke(player);
    }
    
    /// <summary>
    /// Enable/disable interaction at runtime
    /// </summary>
    public void SetCanInteract(bool canInteract)
    {
        _canInteract = canInteract;
    }
    
    /// <summary>
    /// Reset for repeated use
    /// </summary>
    public void Reset()
    {
        _hasBeenUsed = false;
    }
}