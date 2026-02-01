using UnityEngine;

/// <summary>
/// Ranger enemy - keeps distance and shoots at player.
/// Similar to SoulKnight's ranged enemies.
/// Uses a custom kiting behavior but integrates with the state machine.
/// </summary>
public class RangerEnemy : EnemyBase
{
    [Header("Ranger Settings")]
    [SerializeField] private float _preferredDistance = 5f;
    [SerializeField] private float _retreatDistance = 3f;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileSpeed = 8f;
    [SerializeField] private Transform _firePoint;
    
    protected override void Start()
    {
        base.Start();
        
        // Set ranger-specific defaults
        _enemyName = "Ranger";
        _moveSpeed = 2f;
        _attackDamage = 8f;
        _attackRange = 6f;
        _attackCooldown = 1.5f;
        _detectionRange = 8f;
    }
    
    protected override void Update()
    {
        // Update attack cooldown
        if (_attackTimer > 0)
            _attackTimer -= Time.deltaTime;
        
        // Detect player
        DetectPlayer();
        
        if (!Health.IsAlive) return;
        if (Target == null)
        {
            // No target - use state machine for idle/roaming
            _currentState?.Execute(this);
            return;
        }
        
        float distance = GetDistanceToTarget();
        Vector2 dirToTarget = (Target.position - transform.position).normalized;
        
        // Maintain preferred distance (kiting behavior)
        if (distance < _retreatDistance)
        {
            // Too close, retreat
            Vector2 awayDir = -dirToTarget;
            ApplySteering(awayDir);
        }
        else if (distance > _preferredDistance + 1f)
        {
            // Too far, approach
            SmoothMoveToward(Target.position);
        }
        else
        {
            // Good range - stop and face player
            SetVelocity(Vector2.zero);
            FaceDirection(dirToTarget);
            
            // Try to attack if can
            if (CanAttack)
            {
                Attack();
            }
        }
    }
    
    /// <summary>
    /// Override attack to fire a projectile.
    /// </summary>
    public override void Attack()
    {
        if (!CanAttack || Target == null) return;
        if (_projectilePrefab == null)
        {
            Debug.LogWarning("[RangerEnemy] No projectile prefab assigned!");
            return;
        }
        
        _attackTimer = _attackCooldown;
        
        Vector2 firePos = _firePoint != null ? (Vector2)_firePoint.position : (Vector2)transform.position;
        Vector2 direction = ((Vector2)Target.position - firePos).normalized;
        
        // Face the shooting direction
        FaceDirection(direction);
        
        GameObject projectileObj = Instantiate(_projectilePrefab, firePos, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            // Use a layer mask that targets the player
            LayerMask playerLayer = LayerMask.GetMask("Player");
            projectile.Initialize(_attackDamage, direction, _projectileSpeed, playerLayer, gameObject, 2f);
        }
        
        Debug.Log($"[RangerEnemy] Fired projectile at player");
    }
}
