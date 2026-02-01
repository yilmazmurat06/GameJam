using UnityEngine;

/// <summary>
/// Skeleton enemy - SoulKnight style ranged enemy.
/// Keeps distance, shoots projectiles at player.
/// Based on SoulKnight's skeleton archer pattern.
/// </summary>
public class SkeletonEnemy : EnemyBase
{
    [Header("Skeleton Specific")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _projectileSpeed = 8f;
    [SerializeField] private float _preferredDistance = 4f;  // Distance skeleton tries to maintain
    [SerializeField] private float _retreatDistance = 2f;    // If player closer than this, retreat
    [SerializeField] private int _burstCount = 1;            // Shots per attack
    [SerializeField] private float _burstDelay = 0.15f;      // Delay between burst shots
    
    private int _currentBurst;
    private float _burstTimer;
    
    public float PreferredDistance => _preferredDistance;
    public float RetreatDistance => _retreatDistance;
    public bool ShouldRetreat => HasTarget && GetDistanceToTarget() < _retreatDistance;
    
    protected override void Awake()
    {
        base.Awake();
        
        // SoulKnight Skeleton stats
        _enemyName = "Skeleton";
        _moveSpeed = 2.5f;         // Slow
        _attackDamage = 6f;        // Lower damage per hit
        _attackRange = 7f;         // Long range
        _attackCooldown = 1.5f;    // Slower attacks
        _detectionRange = 8f;      // Good detection
        _reactionTime = 0.5f;      // Slower reactions
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Handle burst firing
        if (_currentBurst > 0)
        {
            _burstTimer -= Time.deltaTime;
            if (_burstTimer <= 0)
            {
                FireProjectile();
                _currentBurst--;
                _burstTimer = _burstDelay;
            }
        }
    }
    
    public override void Attack()
    {
        if (!CanAttack || Target == null) return;
        
        // Start burst attack
        _currentBurst = _burstCount;
        _burstTimer = 0; // Fire first shot immediately
        
        // Reset attack timer
        base.Attack(); // This won't do melee damage, just resets cooldown
    }
    
    /// <summary>
    /// Fire a single projectile toward target.
    /// </summary>
    private void FireProjectile()
    {
        if (Target == null || _projectilePrefab == null) return;
        
        Vector2 firePos = _firePoint != null ? _firePoint.position : transform.position;
        Vector2 direction = ((Vector2)Target.position - firePos).normalized;
        
        // Add slight inaccuracy (SoulKnight style)
        float accuracy = 0.95f; // 95% accurate
        direction = AddInaccuracy(direction, 1f - accuracy);
        
        // Spawn projectile
        GameObject projectile = Object.Instantiate(_projectilePrefab, firePos, Quaternion.identity);
        
        // Set projectile velocity
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * _projectileSpeed;
        }
        
        // Rotate projectile to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Set damage on projectile if it has a Projectile component
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Initialize(gameObject, (int)_attackDamage, direction);
        }
        
        Debug.Log($"[{EnemyName}] Fired projectile!");
    }
    
    /// <summary>
    /// Add inaccuracy to aim direction.
    /// </summary>
    private Vector2 AddInaccuracy(Vector2 direction, float inaccuracy)
    {
        float angle = Mathf.Atan2(direction.y, direction.x);
        angle += Random.Range(-inaccuracy, inaccuracy) * Mathf.Deg2Rad * 10f;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
    
    /// <summary>
    /// Get position to maintain preferred distance.
    /// </summary>
    public Vector2 GetPreferredPosition()
    {
        if (Target == null) return transform.position;
        
        Vector2 directionToTarget = GetDirectionToTarget();
        float currentDistance = GetDistanceToTarget();
        
        if (currentDistance < _preferredDistance)
        {
            // Too close, move back
            return (Vector2)transform.position - directionToTarget * (_preferredDistance - currentDistance);
        }
        else if (currentDistance > _preferredDistance + 1f)
        {
            // Too far, move closer
            return (Vector2)transform.position + directionToTarget * (currentDistance - _preferredDistance);
        }
        
        // At good distance, strafe
        Vector2 perpendicular = new Vector2(-directionToTarget.y, directionToTarget.x);
        return (Vector2)transform.position + perpendicular * (Random.value > 0.5f ? 1f : -1f);
    }
}
