using UnityEngine;

/// <summary>
/// Base class for all enemies with FSM-based AI.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected string _enemyName = "Enemy";
    [SerializeField] protected float _moveSpeed = 3f;
    [SerializeField] protected float _attackDamage = 10f;
    [SerializeField] protected float _attackRange = 1f;
    [SerializeField] protected float _attackCooldown = 1f;
    [SerializeField] protected float _attackWindUpTime = 0.5f;
    [SerializeField] protected float _attackRecoveryTime = 1.0f;
    
    [Header("Movement & Reactions")]
    [SerializeField] protected float _acceleration = 5f;
    [SerializeField] protected float _reactionTime = 0.5f;
    
    [Header("Detection")]
    [SerializeField] protected float _detectionRange = 5f;
    [SerializeField] protected LayerMask _playerLayer;
    
    [Header("Components")]
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    
    // Public properties
    public string EnemyName => _enemyName;
    public float MoveSpeed => _moveSpeed;
    public float AttackDamage => _attackDamage;
    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public float DetectionRange => _detectionRange;
    public float Acceleration => _acceleration;
    public float ReactionTime => _reactionTime;
    public float AttackWindUpTime => _attackWindUpTime;
    public float AttackRecoveryTime => _attackRecoveryTime;
    public Rigidbody2D Rigidbody { get; private set; }
    public Health Health { get; private set; }
    public Transform Target { get; private set; }
    public bool HasTarget => Target != null;
    
    // State machine
    protected IEnemyState _currentState;
    protected float _attackTimer;
    
    public bool CanAttack => _attackTimer <= 0;
    
    protected virtual void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Health = GetComponent<Health>();
        
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Disable gravity for top-down
        Rigidbody.gravityScale = 0f;
    }
    
    protected virtual void Start()
    {
        // Subscribe to death event
        Health.OnDeath += HandleDeath;
        
        // Start in idle state
        ChangeState(new EnemyIdleState());
    }
    
    protected virtual void OnDestroy()
    {
        if (Health != null)
            Health.OnDeath -= HandleDeath;
    }
    
    protected virtual void Update()
    {
        // Update attack cooldown
        if (_attackTimer > 0)
            _attackTimer -= Time.deltaTime;
        
        // Detect player
        DetectPlayer();
        
        // Execute current state
        _currentState?.Execute(this);
    }
    
    /// <summary>
    /// Change to a new state.
    /// </summary>
    public void ChangeState(IEnemyState newState)
    {
        if (newState == null) return;
        
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }
    
    /// <summary>
    /// Set velocity for movement.
    /// </summary>
    public void SetVelocity(Vector2 velocity)
    {
        Rigidbody.linearVelocity = velocity;
    }
    
    /// <summary>
    /// Move toward a target position with smoothing / acceleration.
    /// </summary>
    public void SmoothMoveToward(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        Vector2 targetVelocity = direction * _moveSpeed;
        
        // Smoothly interpolate current velocity to target velocity
        Rigidbody.linearVelocity = Vector2.Lerp(Rigidbody.linearVelocity, targetVelocity, _acceleration * Time.deltaTime);
        
        // Face target
        if (direction.x != 0 && _spriteRenderer != null)
        {
            _spriteRenderer.flipX = direction.x < 0;
        }
        
        // Update Animator if exists
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsMoving", Rigidbody.linearVelocity.magnitude > 0.1f);
            if (direction != Vector2.zero)
            {
                animator.SetFloat("MoveX", direction.x);
                animator.SetFloat("MoveY", direction.y);
            }
        }
    }

    /// <summary>
    /// Move toward a target position (Direct Velocity).
    /// </summary>
    public void MoveToward(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        SetVelocity(direction * _moveSpeed);
        
        // Face target
        if (direction.x != 0 && _spriteRenderer != null)
        {
            _spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    /// <summary>
    /// Perform an attack.
    /// </summary>
    public virtual void Attack()
    {
        if (!CanAttack || Target == null) return;
        
        _attackTimer = _attackCooldown;
        
        // Check if in range
        float distance = Vector2.Distance(transform.position, Target.position);
        if (distance <= _attackRange)
        {
            IDamageable damageable = Target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 direction = (Target.position - transform.position).normalized;
                DamageInfo damageInfo = new DamageInfo(_attackDamage, gameObject, DamageType.Physical)
                    .WithKnockback(direction * 3f)
                    .WithHitPoint(transform.position);
                
                damageable.TakeDamage(damageInfo);
                Debug.Log($"[{_enemyName}] Attacked player for {_attackDamage} damage");
            }
        }
    }
    
    /// <summary>
    /// Detect the player within range.
    /// </summary>
    protected virtual void DetectPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, _detectionRange, _playerLayer);
        
        if (playerCollider != null)
        {
            Target = playerCollider.transform;
        }
        else
        {
            Target = null;
        }
    }
    
    /// <summary>
    /// Get distance to current target.
    /// </summary>
    public float GetDistanceToTarget()
    {
        if (Target == null) return float.MaxValue;
        return Vector2.Distance(transform.position, Target.position);
    }
    
    /// <summary>
    /// Check if target is within attack range.
    /// </summary>
    public bool IsTargetInAttackRange()
    {
        return GetDistanceToTarget() <= _attackRange;
    }
    
    protected virtual void HandleDeath()
    {
        ChangeState(new EnemyDeathState());
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}
