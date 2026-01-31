using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input using Unity's new Input System.
/// Falls back to legacy Input.GetKey() if Input Actions are not assigned.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    // Input values
    private float _horizontalInput;
    private bool _jumpPressed;
    private bool _interactPressed;
    
    // Track if using legacy input
    private bool _useLegacyInput = false;
    
    // Public accessors
    public float HorizontalInput => _horizontalInput;
    
    public bool JumpPressed
    {
        get
        {
            bool value = _jumpPressed;
            _jumpPressed = false; // Consume on read
            return value;
        }
    }
    
    public bool InteractPressed
    {
        get
        {
            bool value = _interactPressed;
            _interactPressed = false; // Consume on read
            return value;
        }
    }
    
    // Input Action references (assign in Inspector or via code)
    [Header("Input Actions (Optional - falls back to legacy input)")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference interactAction;
    
    [Header("Legacy Input Keys")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    private void OnEnable()
    {
        // Check if we should use legacy input
        _useLegacyInput = (moveAction == null && jumpAction == null && interactAction == null);
        
        if (_useLegacyInput)
        {
            Debug.Log("[PlayerInputHandler] Using legacy Input (WASD/Space/E)");
            return;
        }
        
        // Enable actions and subscribe
        if (moveAction != null)
        {
            moveAction.action.Enable();
            moveAction.action.performed += OnMove;
            moveAction.action.canceled += OnMove;
        }
        
        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJump;
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
        
        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJump;
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
            // Horizontal input (A/D or Left/Right arrows)
            _horizontalInput = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                _horizontalInput = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                _horizontalInput = 1f;
        }
        
        if (_useLegacyInput || jumpAction == null)
        {
            // Jump
            if (Input.GetKeyDown(jumpKey))
                _jumpPressed = true;
        }
        
        if (_useLegacyInput || interactAction == null)
        {
            // Interact
            if (Input.GetKeyDown(interactKey))
                _interactPressed = true;
        }
    }
    
    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        _horizontalInput = input.x;
    }
    
    private void OnJump(InputAction.CallbackContext context)
    {
        _jumpPressed = true;
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
        _horizontalInput = 0f;
        _jumpPressed = false;
        _interactPressed = false;
    }
}