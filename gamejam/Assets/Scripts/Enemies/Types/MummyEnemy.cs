using UnityEngine;

/// <summary>
/// Mummy enemy - basic melee chaser.
/// Similar to SoulKnight's basic melee enemies.
/// Uses the state machine from EnemyBase for AI behavior.
/// </summary>
public class MummyEnemy : EnemyBase
{
    [Header("Mummy Settings")]
    [SerializeField] private float _lungeForce = 3f;
    [SerializeField] private float _attackRadius = 0.8f;
    
    protected override void Start()
    {
        base.Start();
        
        // Set mummy-specific defaults
        _enemyName = "Mummy";
        _moveSpeed = 2.5f;
        _attackDamage = 12f;
        _attackRange = 1.2f;
        _attackCooldown = 1.2f;
        _attackWindUpTime = 0.3f;
        _detectionRange = 6f;
    }
    
    // MummyEnemy relies entirely on the state machine from EnemyBase
    // No need to override Update() - let the FSM handle movement and attacks
    
    /// <summary>
    /// Override attack to perform a melee swing with area damage.
    /// </summary>
    public override void Attack()
    {
        if (!CanAttack || Target == null) return;
        
        _attackTimer = _attackCooldown;
        
        // Small lunge towards player
        Vector2 direction = ((Vector2)Target.position - (Vector2)transform.position).normalized;
        Rigidbody.AddForce(direction * _lungeForce, ForceMode2D.Impulse);
        
        // Deal damage in attack radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _attackRadius, _playerLayer);
        
        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                DamageInfo damage = new DamageInfo(_attackDamage, gameObject, DamageType.Physical)
                    .WithKnockback(knockbackDir * 4f)
                    .WithHitPoint(transform.position);
                
                damageable.TakeDamage(damage);
                Debug.Log($"[MummyEnemy] Hit player for {_attackDamage} damage!");
            }
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw attack radius
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);
    }
}
