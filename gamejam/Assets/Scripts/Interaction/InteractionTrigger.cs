using UnityEngine;

/// <summary>
/// Attached to the player to detect interactables in range.
/// Calls interact when player presses the interact key.
/// </summary>
public class InteractionTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _interactionRadius = 1.5f;
    [SerializeField] private LayerMask _interactableLayer;
    
    [Header("References")]
    [SerializeField] private PlayerController _player;
    [SerializeField] private PlayerInputHandler _inputHandler;
    
    // Currently detected interactable
    private IInteractable _currentInteractable;
    private Collider2D _currentCollider;
    
    // UI prompt (can be connected to a UI element)
    public IInteractable CurrentInteractable => _currentInteractable;
    public bool HasInteractable => _currentInteractable != null && _currentInteractable.CanInteract;
    public string CurrentPrompt => _currentInteractable?.InteractionPrompt ?? "";
    
    private void Awake()
    {
        if (_player == null)
            _player = GetComponent<PlayerController>();
        if (_inputHandler == null)
            _inputHandler = GetComponent<PlayerInputHandler>();
    }
    
    private void Update()
    {
        // Find nearby interactables
        DetectInteractables();
        
        // Check for interact input
        if (_inputHandler != null && _inputHandler.InteractPressed)
        {
            TryInteract();
        }
    }
    
    private void DetectInteractables()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _interactionRadius, _interactableLayer);
        
        IInteractable closest = null;
        Collider2D closestCollider = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider2D col in colliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract)
            {
                float distance = Vector2.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = interactable;
                    closestCollider = col;
                }
            }
        }
        
        // Update current
        if (closest != _currentInteractable)
        {
            _currentInteractable = closest;
            _currentCollider = closestCollider;
            
            // Could trigger UI update here
            // OnInteractableChanged?.Invoke(_currentInteractable);
        }
    }
    
    private void TryInteract()
    {
        if (_currentInteractable != null && _currentInteractable.CanInteract)
        {
            _currentInteractable.OnInteract(_player);
            GameEvents.TriggerPlayerInteract(_currentInteractable);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _interactionRadius);
    }
}