using UnityEngine;

/// <summary>
/// Charger enemy - rushes directly at the player for contact damage.
/// Similar to SoulKnight's charging enemies.
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
        _attackRange = 0.5f; // Contact damage
        _attackCooldown = 2f;
        _detectionRange = 7f;
        
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (!Health.IsAlive) return;
        
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
        
        // Normal behavior: approach player and initiate charge when in range
        if (Target != null)
        {
            float distance = GetDistanceToTarget();
            
            if (distance <= _attackRange + 0.5f && CanAttack)
            {
                // In attack range, start wind up for charge
                StartWindup();
            }
            else if (distance <= _detectionRange)
            {
                // Chase player
                SmoothMoveToward(Target.position);
            }
        }
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
        }
        
        // Visual feedback - flash color
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _chargeColor;
        }
    }
    
    private void HandleWindup()
    {
        _chargeTimer -= Time.deltaTime;
        
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
        
        SetVelocity(_chargeDirection * _chargeSpeed);
        Debug.Log("[ChargerEnemy] Charging!");
    }
    
    private void HandleCharge()
    {
        _chargeTimer -= Time.deltaTime;
        
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
