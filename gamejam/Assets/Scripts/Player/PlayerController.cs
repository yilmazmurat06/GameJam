using UnityEngine;

/// <summary>
/// Mask types representing emotional stages of relationship collapse.
/// </summary>
public enum MaskType
{
    None,
    Fear,    // KORKU - Glass cannon, shadow form
    Hate,    // NEFRET - Damage reduction, shield bash
    Sorrow,  // HÜZÜN - Phasing ability
    Guilt    // SUÇLULUK - Heavy anchor
}

/// <summary>
/// Main player controller using a Finite State Machine pattern.
/// Manages state transitions, physics, and responds to game events.
/// Refactored for 8-directional top-down movement.
/// Now includes SoulKnight-style dual-weapon system.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Energy))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _baseMoveSpeed = 5f;
    
    [Header("Mask System")]
    [SerializeField] private MaskType _currentMask = MaskType.None;
    
    [Header("Dual Weapon System")]
    [SerializeField] private WeaponBase _weapon1;
    [SerializeField] private WeaponBase _weapon2;
    [SerializeField] private int _activeWeaponSlot = 0; // 0 = weapon1, 1 = weapon2
    
    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    // Public properties
    public float MoveSpeed => _moveSpeed;
    public MaskType CurrentMask => _currentMask;
    public WeaponBase CurrentWeapon => _activeWeaponSlot == 0 ? _weapon1 : _weapon2;
    public WeaponBase Weapon1 => _weapon1;
    public WeaponBase Weapon2 => _weapon2;
    public int ActiveWeaponSlot => _activeWeaponSlot;
    public Rigidbody2D Rigidbody { get; private set; }
    public PlayerInputHandler InputHandler { get; private set; }
    public Health Health { get; private set; }
    public Energy Energy { get; private set; }
    public Animator Animator { get; private set; }
    
    // State machine
    private IPlayerState _currentState;
    private IPlayerState _previousState;
    
    // Mask Ability
    private MaskAbility _currentAbility;
    
    // Store state before cutscene to return to it
    private IPlayerState _stateBeforeFrozen;
    
    // Track last facing direction for top-down
    private bool _facingRight = true;
    
    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        InputHandler = GetComponent<PlayerInputHandler>();
        Health = GetComponent<Health>();
        Energy = GetComponent<Energy>();
        Animator = GetComponent<Animator>(); // May be null
        
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Disable gravity for top-down movement
        Rigidbody.gravityScale = 0f;
        Rigidbody.bodyType = RigidbodyType2D.Dynamic; // Ensure it can move
        Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation; // Don't spin
        
        // Store base speed
        _baseMoveSpeed = _moveSpeed;
    }
    
    private void Start()
    {
        // Initialize animator to face DOWN
        if (Animator != null)
        {
            Animator.SetFloat("MoveX", 0f);
            Animator.SetFloat("MoveY", -1f);
        }
        
        // Start in idle state
        ChangeState(new PlayerIdleState());
        
        // Apply initial mask effects
        ApplyMaskEffects();
        
        // Ensure we are on the Player layer
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1) gameObject.layer = playerLayer;
        
        // Subscribe to death
        Health.OnDeath += HandleDeath;
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
        
        if (Health != null)
            Health.OnDeath -= HandleDeath;
    }
    
    private void Update()
    {
        // Execute current state
        _currentState?.Execute(this);
        
        // Handle Weapon Swap
        if (InputHandler.WeaponSwapPressed)
        {
            SwapWeapon();
        }
        
        // Handle Attack Input
        if (InputHandler.AttackPressed || InputHandler.AttackHeld)
        {
            TryAttack();
        }
        
        // Handle Ability Input (independent of state for now)
        if (InputHandler.AbilityPressed && _currentAbility != null)
        {
            _currentAbility.Activate();
        }
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
    /// Sets velocity for 8-directional top-down movement.
    /// </summary>
    public void SetVelocity(Vector2 velocity)
    {
        Rigidbody.velocity = velocity;
    }
    
    /// <summary>
    /// Sets sprite facing direction (left/right flip for top-down).
    /// Only updates if horizontal input is non-zero to maintain last direction.
    /// </summary>
    public void SetFacingDirection(bool facingRight)
    {
        _facingRight = facingRight;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.flipX = !facingRight;
        }
    }
    
    /// <summary>
    /// Gets the current facing direction.
    /// </summary>
    public bool IsFacingRight => _facingRight;
    
    /// <summary>
    /// Changes the current mask and applies its effects.
    /// </summary>
    public void SetMask(MaskType newMask)
    {
        if (_currentMask == newMask) return;
        
        _currentMask = newMask;
        ApplyMaskEffects();
        UpdateMaskAbility();
    }
    
    private void UpdateMaskAbility()
    {
        // Remove existing ability
        if (_currentAbility != null)
        {
            Destroy(_currentAbility);
            _currentAbility = null;
        }
        
        // Add new ability component
        switch (_currentMask)
        {
            case MaskType.Fear:
                _currentAbility = gameObject.AddComponent<FearAbility>();
                break;
            case MaskType.Hate:
                _currentAbility = gameObject.AddComponent<HateAbility>();
                break;
            case MaskType.Sorrow:
                _currentAbility = gameObject.AddComponent<SorrowAbility>();
                break;
            case MaskType.Guilt:
                _currentAbility = gameObject.AddComponent<GuiltAbility>();
                break;
        }
        
        if (_currentAbility != null)
        {
            Debug.Log($"[PlayerController] Added ability: {_currentAbility.GetType().Name}");
        }
    }
    
    /// <summary>
    /// Applies effects based on the current mask type.
    /// </summary>
    public void ApplyMaskEffects()
    {
        // Reset to base values first
        _moveSpeed = _baseMoveSpeed;
        Rigidbody.mass = 100f;
        Time.timeScale = 1f;
        
        // Reset sprite alpha
        if (_spriteRenderer != null)
        {
            Color c = _spriteRenderer.color;
            c.a = 1f;
            _spriteRenderer.color = c;
        }
        
        switch (_currentMask)
        {
            case MaskType.None:
                // Default values - no modifications
                break;
                
            case MaskType.Fear:
                // KORKU: Glass cannon - 1 HP (handled by Health component)
                // Passive: Extreme vulnerability
                if (Health != null) Health.SetMaxHealth(1);
                break;
                
            case MaskType.Hate:
                // NEFRET: 30% damage reduction, aggressive
                // Passive: Damage resistance
                if (Health != null) Health.SetDamageMultiplier(0.7f);
                break;
                
            case MaskType.Sorrow:
                // HÜZÜN: 50% alpha, ethereal
                // Passive: Semi-transparent
                if (_spriteRenderer != null)
                {
                    Color c = _spriteRenderer.color;
                    c.a = 0.5f;
                    _spriteRenderer.color = c;
                }
                break;
                
            case MaskType.Guilt:
                // SUÇLULUK: -50% speed, heavy
                // Passive: Slow movement
                _moveSpeed = _baseMoveSpeed * 0.5f;
                Rigidbody.mass = 200f;
                break;
        }
        
        Debug.Log($"[PlayerController] Mask: {_currentMask} | Speed: {_moveSpeed} | Mass: {Rigidbody.mass}");
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
        ChangeState(new PlayerIdleState());
    }
    
    private void HandleDeath()
    {
        Debug.Log("[PlayerController] Player died!");
        ChangeState(new PlayerFrozenState());
        // TODO: Trigger game over screen
    }
    
    /// <summary>
    /// Equip a new weapon to a specific slot.
    /// </summary>
    public void EquipWeapon(WeaponBase weapon, int slot = -1)
    {
        if (slot < 0) slot = _activeWeaponSlot;
        
        if (slot == 0)
            _weapon1 = weapon;
        else
            _weapon2 = weapon;
            
        Debug.Log($"[PlayerController] Equipped {weapon?.WeaponName ?? "nothing"} to slot {slot}");
    }
    
    /// <summary>
    /// Swap between weapon slots.
    /// </summary>
    public void SwapWeapon()
    {
        _activeWeaponSlot = _activeWeaponSlot == 0 ? 1 : 0;
        Debug.Log($"[PlayerController] Weapon swapped to slot {_activeWeaponSlot}: {CurrentWeapon?.WeaponName ?? "empty"}");
    }
    
    /// <summary>
    /// Attempt to attack with current weapon, consuming energy if needed.
    /// </summary>
    public void TryAttack()
    {
        WeaponBase weapon = CurrentWeapon;
        if (weapon == null) return;
        if (!weapon.CanAttack) return;
        
        // Check energy cost
        float energyCost = weapon.EnergyCost;
        if (energyCost > 0 && !Energy.CanAfford(energyCost))
        {
            Debug.Log($"[PlayerController] Not enough energy for {weapon.WeaponName}");
            return;
        }
        
        // Consume energy and attack
        if (energyCost > 0)
        {
            Energy.Consume(energyCost);
        }
        
        weapon.Attack();
    }
}