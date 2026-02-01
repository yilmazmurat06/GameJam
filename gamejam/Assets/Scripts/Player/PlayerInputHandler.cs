using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input using Unity's new Input System.
/// Falls back to legacy Input.GetKey() if Input Actions are not assigned.
/// Supports 8-directional top-down movement.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    // Input values
    private Vector2 _moveInput;
    private bool _interactPressed;
    private bool _attackPressed;
    private bool _attackHeld;
    private bool _weaponSwapPressed;
    
    // Track if using legacy input
    private bool _useLegacyInput = false;
    
    // Public accessors
    /// <summary>
    /// Returns normalized 8-directional movement input.
    /// </summary>
    public Vector2 MoveInput => _moveInput.normalized;
    
    /// <summary>
    /// Raw horizontal input (for backwards compatibility).
    /// </summary>
    public float HorizontalInput => _moveInput.x;
    
    /// <summary>
    /// Raw vertical input.
    /// </summary>
    public float VerticalInput => _moveInput.y;
    
    public bool InteractPressed
    {
        get
        {
            bool value = _interactPressed;
            _interactPressed = false; // Consume on read
            return value;
        }
    }
    
    public bool AttackPressed
    {
        get
        {
            bool value = _attackPressed;
            _attackPressed = false; // Consume on read
            return value;
        }
    }
    
    /// <summary>
    /// Is attack button currently held (for automatic weapons).
    /// </summary>
    public bool AttackHeld => _attackHeld;
    
    /// <summary>
    /// Was weapon swap button pressed this frame.
    /// </summary>
    public bool WeaponSwapPressed
    {
        get
        {
            bool value = _weaponSwapPressed;
            _weaponSwapPressed = false; // Consume on read
            return value;
        }
    }
    
    /// <summary>
    /// Returns aim direction based on mouse position relative to player.
    /// </summary>
    public Vector2 AimDirection
    {
        get
        {
            Camera cam = Camera.main;
            if (cam == null) return Vector2.right;
            
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mouseWorld - transform.position).normalized;
            return direction == Vector2.zero ? Vector2.right : direction;
        }
    }
    
    // Input Action references (assign in Inspector or via code)
    [Header("Input Actions (Optional - falls back to legacy input)")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference interactAction;
    
    [Header("Legacy Input Keys")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode attackKey = KeyCode.Space;
    [SerializeField] private KeyCode abilityKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode weaponSwapKey = KeyCode.Q;

    private bool _abilityPressed;

    public bool AbilityPressed
    {
        get
        {
            bool value = _abilityPressed;
            _abilityPressed = false; // Consume on read
            return value;
        }
    }
    
    private void OnEnable()
    {
        // Check if we should use legacy input
        _useLegacyInput = (moveAction == null && interactAction == null);
        
        if (_useLegacyInput)
        {
            Debug.Log("[PlayerInputHandler] Using legacy Input (WASD/E)");
            return;
        }
        
        // Enable actions and subscribe
        if (moveAction != null)
        {
            moveAction.action.Enable();
            moveAction.action.performed += OnMove;
            moveAction.action.canceled += OnMove;
        }
        
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteract;
        }
    }
    
    private void OnDisable()
    {
        if (_useLegacyInput) return;
        
        // Unsubscribe
        if (moveAction != null)
        {
            moveAction.action.performed -= OnMove;
            moveAction.action.canceled -= OnMove;
        }
        
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
        }
    }
    
    private void Update()
    {
        // Legacy input fallback
        if (_useLegacyInput || moveAction == null)
        {
            // 8-directional input (WASD or Arrow keys)
            float horizontal = 0f;
            float vertical = 0f;
            
            // Horizontal
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                horizontal = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                horizontal = 1f;
            
            // Vertical
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                vertical = 1f;
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                vertical = -1f;
            
            _moveInput = new Vector2(horizontal, vertical);
            
            // Interact
            if (Input.GetKeyDown(interactKey))
                _interactPressed = true;
            
            // Attack (Space or Mouse)
            if (Input.GetKeyDown(attackKey) || Input.GetMouseButtonDown(0))
                _attackPressed = true;
            
            // Attack held (for automatic weapons)
            _attackHeld = Input.GetKey(attackKey) || Input.GetMouseButton(0);
                
            // Ability (Shift)
            if (Input.GetKeyDown(abilityKey))
                _abilityPressed = true;
            
            // Weapon Swap (Q)
            if (Input.GetKeyDown(weaponSwapKey))
                _weaponSwapPressed = true;
        }
    }
    

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }
    
    private void OnInteract(InputAction.CallbackContext context)
    {
        _interactPressed = true;
    }
    
    /// <summary>
    /// Clears all input (used when freezing player)
    /// </summary>
    public void ClearInput()
    {
        _moveInput = Vector2.zero;
        _interactPressed = false;
        _attackPressed = false;
        _attackHeld = false;
        _abilityPressed = false;
        _weaponSwapPressed = false;
    }
}