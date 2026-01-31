using UnityEngine;

/// <summary>
/// Main player controller using a Finite State Machine pattern.
/// Manages state transitions, physics, and responds to game events.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _airControlMultiplier = 0.8f;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask _groundLayer;
    
    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    // Public properties
    public float MoveSpeed => _moveSpeed;
    public float JumpForce => _jumpForce;
    public float AirControlMultiplier => _airControlMultiplier;
    public bool IsGrounded { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public PlayerInputHandler InputHandler { get; private set; }
    public Animator Animator { get; private set; }
    
    // State machine
    private IPlayerState _currentState;
    private IPlayerState _previousState;
    
    // Store state before cutscene to return to it
    private IPlayerState _stateBeforeFrozen;
    
    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        InputHandler = GetComponent<PlayerInputHandler>();
        Animator = GetComponent<Animator>(); // May be null
        
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Create ground check if not assigned
        if (_groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            _groundCheck = groundCheckObj.transform;
        }
    }
    
    private void Start()
    {
        // Start in idle state
        ChangeState(new PlayerIdleState());
    }
    
    private void OnEnable()
    {
        // Subscribe to cutscene events
        GameEvents.OnCutsceneStart += HandleCutsceneStart;
        GameEvents.OnCutsceneEnd += HandleCutsceneEnd;
    }
    
    private void OnDisable()
    {
        GameEvents.OnCutsceneStart -= HandleCutsceneStart;
        GameEvents.OnCutsceneEnd -= HandleCutsceneEnd;
    }
    
    private void Update()
    {
        // Update ground check
        IsGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        
        // Execute current state
        _currentState?.Execute(this);
    }
    
    /// <summary>
    /// Changes to a new state, calling Exit on current and Enter on new.
    /// </summary>
    public void ChangeState(IPlayerState newState)
    {
        if (newState == null) return;
        
        _previousState = _currentState;
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }
    
    /// <summary>
    /// Sets horizontal velocity while preserving vertical velocity.
    /// </summary>
    public void SetHorizontalVelocity(float velocity)
    {
        Rigidbody.linearVelocity = new Vector2(velocity, Rigidbody.linearVelocity.y);
    }
    
    /// <summary>
    /// Applies jump force as an impulse.
    /// </summary>
    public void ApplyJumpForce()
    {
        Rigidbody.linearVelocity = new Vector2(Rigidbody.linearVelocity.x, 0f);
        Rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
    }
    
    /// <summary>
    /// Sets sprite facing direction.
    /// </summary>
    public void SetFacingDirection(bool facingRight)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.flipX = !facingRight;
        }
    }
    
    // --- Cutscene Handlers ---
    
    private void HandleCutsceneStart()
    {
        // Store current state and freeze
        _stateBeforeFrozen = _currentState;
        InputHandler.ClearInput();
        ChangeState(new PlayerFrozenState());
    }
    
    private void HandleCutsceneEnd()
    {
        // Return to previous state or idle
        if (_stateBeforeFrozen != null)
        {
            ChangeState(new PlayerIdleState()); // Safe default
        }
        else
        {
            ChangeState(new PlayerIdleState());
        }
    }
    
    // --- Debug Visualization ---
    
    private void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }
}