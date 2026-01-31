using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input using Unity's new Input System.
/// Provides abstracted input values consumed by PlayerController states.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    // Input values
    private float _horizontalInput;
    private bool _jumpPressed;
    private bool _interactPressed;
    
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
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference interactAction;
    
    private void OnEnable()
    {
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