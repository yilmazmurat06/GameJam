using UnityEngine;

/// <summary>
/// Base class for mask active abilities.
/// Each mask (Fear, Hate, Sorrow, Guilt) has an active ability.
/// </summary>
public abstract class MaskAbility : MonoBehaviour
{
    [Header("Ability Settings")]
    [SerializeField] protected float _cooldown = 5f;
    [SerializeField] protected float _duration = 3f;
    [SerializeField] protected KeyCode _activationKey = KeyCode.LeftShift;
    
    protected float _cooldownTimer;
    protected float _durationTimer;
    protected bool _isActive;
    
    public bool IsActive => _isActive;
    public bool IsOnCooldown => _cooldownTimer > 0;
    public float CooldownRemaining => _cooldownTimer;
    public float DurationRemaining => _durationTimer;
    
    protected PlayerController _player;
    
    protected virtual void Awake()
    {
        _player = GetComponent<PlayerController>();
    }
    
    protected virtual void Update()
    {
        // Handle cooldown
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }
        
        // Handle duration
        if (_isActive)
        {
            _durationTimer -= Time.deltaTime;
            if (_durationTimer <= 0)
            {
                Deactivate();
            }
        }
        
        // Check for activation input
        if (Input.GetKeyDown(_activationKey) && !IsOnCooldown && !_isActive)
        {
            Activate();
        }
    }
    
    /// <summary>
    /// Activate the ability.
    /// </summary>
    public virtual void Activate()
    {
        if (IsOnCooldown || _isActive) return;
        
        _isActive = true;
        _durationTimer = _duration;
        
        Debug.Log($"[MaskAbility] {GetType().Name} activated for {_duration}s");
        OnActivate();
    }
    
    /// <summary>
    /// Deactivate the ability.
    /// </summary>
    public virtual void Deactivate()
    {
        if (!_isActive) return;
        
        _isActive = false;
        _cooldownTimer = _cooldown;
        
        Debug.Log($"[MaskAbility] {GetType().Name} deactivated. Cooldown: {_cooldown}s");
        OnDeactivate();
    }
    
    /// <summary>
    /// Override for custom activation logic.
    /// </summary>
    protected abstract void OnActivate();
    
    /// <summary>
    /// Override for custom deactivation logic.
    /// </summary>
    protected abstract void OnDeactivate();
}
