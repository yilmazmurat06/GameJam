using UnityEngine;

/// <summary>
/// Charger enemy - rushes directly at the player for contact damage.
/// Similar to SoulKnight's charging enemies.
/// Uses a custom charge behavior but integrates with the state machine.
/// </summary>
public class ChargerEnemy : EnemyBase
{
    [Header("Charger Settings")]
    [SerializeField] private float _chargeSpeed = 8f;
    [SerializeField] private float _chargeDuration = 0.5f;
    [SerializeField] private float _chargeWindup = 0.3f;
    [SerializeField] private Color _chargeColor = Color.red;
    
    private bool _isCharging = false;
    private bool _isWindingUp = false;
    private float _chargeTimer = 0f;
    private Vector2 _chargeDirection;
    private Color _originalColor;
    
    protected override void Start()
    {
        base.Start();
        
        // Set charger-specific defaults
        _enemyName = "Charger";
        _moveSpeed = 3f;
        _attackDamage = 15f;
        _attackRange = 2f; // Larger range to trigger charge
        _attackCooldown = 2f;
        _detectionRange = 7f;
        
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
    }
    
    protected override void Update()
    {
        // Handle special charge behavior
        if (_isCharging)
        {
            HandleCharge();
            return;
        }
        
        if (_isWindingUp)
        {
            HandleWindup();
            return;
        }
        
        // Run normal state machine when not charging
        base.Update();
    }
    
    /// <summary>
    /// Override attack to start charge wind-up.
    /// </summary>
    public override void Attack()
    {
        if (!CanAttack || Target == null || _isCharging || _isWindingUp) return;
        
        StartWindup();
    }
    
    private void StartWindup()
    {
        _isWindingUp = true;
        _chargeTimer = _chargeWindup;
        SetVelocity(Vector2.zero);
        
        // Store charge direction
        if (Target != null)
        {
            _chargeDirection = ((Vector2)Target.position - (Vector2)transform.position).normalized;
            // Face the charge direction
            FaceDirection(_chargeDirection);
        }
        
        // Visual feedback - flash color
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _chargeColor;
        }
        
        Debug.Log("[ChargerEnemy] Winding up charge!");
    }
    
    private void HandleWindup()
    {
        _chargeTimer -= Time.deltaTime;
        
        // Face target during windup (allows small aim adjustment)
        if (Target != null)
        {
            _chargeDirection = ((Vector2)Target.position - (Vector2)transform.position).normalized;
            FaceDirection(_chargeDirection);
        }
        
        if (_chargeTimer <= 0)
        {
            _isWindingUp = false;
            StartCharge();
        }
    }
    
    private void StartCharge()
    {
        _isCharging = true;
        _chargeTimer = _chargeDuration;
        _attackTimer = _attackCooldown;
        
        // Apply charge velocity with animator update
        Rigidbody.linearVelocity = _chargeDirection * _chargeSpeed;
        if (_enemyAnimator != null)
        {
            _enemyAnimator.SetDirection(_chargeDirection);
        }
        
        Debug.Log("[ChargerEnemy] Charging!");
    }
    
    private void HandleCharge()
    {
        _chargeTimer -= Time.deltaTime;
        
        // Keep animator updated during charge
        if (_enemyAnimator != null)
        {
            _enemyAnimator.SetDirection(_chargeDirection);
        }
        
        if (_chargeTimer <= 0)
        {
            EndCharge();
        }
    }
    
    private void EndCharge()
    {
        _isCharging = false;
        SetVelocity(Vector2.zero);
        
        // Reset color
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }
        
        Debug.Log("[ChargerEnemy] Charge ended");
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Deal contact damage when charging
        if (_isCharging)
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                DamageInfo damage = new DamageInfo(_attackDamage, gameObject, DamageType.Physical)
                    .WithKnockback(knockbackDir * 5f)
                    .WithHitPoint(collision.contacts[0].point);
                
                damageable.TakeDamage(damage);
                Debug.Log($"[ChargerEnemy] Dealt {_attackDamage} charge damage!");
                
                EndCharge();
            }
        }
    }
}
