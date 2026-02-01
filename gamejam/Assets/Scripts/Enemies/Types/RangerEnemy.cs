using UnityEngine;

/// <summary>
/// Ranger enemy - keeps distance and shoots at player.
/// Similar to SoulKnight's ranged enemies.
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
        base.Update();
        
        if (!Health.IsAlive) return;
        if (Target == null) return;
        
        float distance = GetDistanceToTarget();
        
        // Maintain preferred distance
        if (distance < _retreatDistance)
        {
            // Too close, retreat
            Vector2 awayDir = ((Vector2)transform.position - (Vector2)Target.position).normalized;
            ApplySteering(awayDir);
        }
        else if (distance > _preferredDistance + 1f)
        {
            // Too far, approach
            SmoothMoveToward(Target.position);
        }
        else
        {
            // Good range, stop and shoot
            SetVelocity(Vector2.zero);
            
            // Face player
            Vector2 dir = (Target.position - transform.position).normalized;
            if (_spriteRenderer != null && dir.x != 0)
            {
                _spriteRenderer.flipX = dir.x < 0;
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
