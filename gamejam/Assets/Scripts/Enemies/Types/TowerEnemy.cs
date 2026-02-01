using UnityEngine;

/// <summary>
/// Tower enemy - stationary turret that fires at player.
/// Similar to SoulKnight's tower enemies.
/// </summary>
public class TowerEnemy : EnemyBase
{
    [Header("Tower Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileSpeed = 6f;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _rotationSpeed = 180f; // Degrees per second
    [SerializeField] private int _burstCount = 3;
    [SerializeField] private float _burstDelay = 0.15f;
    
    private float _burstTimer = 0f;
    private int _currentBurst = 0;
    private bool _isBursting = false;
    
    protected override void Start()
    {
        base.Start();
        
        // Set tower-specific defaults
        _enemyName = "Tower";
        _moveSpeed = 0f; // Stationary
        _attackDamage = 6f;
        _attackRange = 10f;
        _attackCooldown = 2f;
        _detectionRange = 10f;
        
        // Towers don't move, freeze rigidbody
        if (Rigidbody != null)
        {
            Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        }
    }
    
    protected override void Update()
    {
        // Don't call base.Update's state machine - towers have simpler behavior
        if (_attackTimer > 0)
            _attackTimer -= Time.deltaTime;
        
        DetectPlayer();
        
        if (!Health.IsAlive) return;
        if (Target == null) return;
        
        // Rotate to face player
        RotateTowardsTarget();
        
        // Handle burst firing
        if (_isBursting)
        {
            HandleBurst();
            return;
        }
        
        // Fire when ready
        if (CanAttack && IsTargetInAttackRange())
        {
            StartBurst();
        }
    }
    
    private void RotateTowardsTarget()
    {
        if (Target == null) return;
        
        Vector2 direction = (Target.position - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Smooth rotation
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, _rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }
    
    private void StartBurst()
    {
        _isBursting = true;
        _currentBurst = 0;
        _burstTimer = 0f;
        _attackTimer = _attackCooldown;
    }
    
    private void HandleBurst()
    {
        _burstTimer -= Time.deltaTime;
        
        if (_burstTimer <= 0)
        {
            FireProjectile();
            _currentBurst++;
            _burstTimer = _burstDelay;
            
            if (_currentBurst >= _burstCount)
            {
                _isBursting = false;
            }
        }
    }
    
    private void FireProjectile()
    {
        if (_projectilePrefab == null)
        {
            Debug.LogWarning("[TowerEnemy] No projectile prefab assigned!");
            return;
        }
        
        Vector2 firePos = _firePoint != null ? (Vector2)_firePoint.position : (Vector2)transform.position;
        Vector2 direction = transform.right; // Fire in facing direction
        
        GameObject projectileObj = Instantiate(_projectilePrefab, firePos, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            LayerMask playerLayer = LayerMask.GetMask("Player");
            projectile.Initialize(_attackDamage, direction, _projectileSpeed, playerLayer, gameObject, 1f);
        }
    }
    
    // Towers don't use the base Attack method
    public override void Attack() { }
}
