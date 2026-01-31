using UnityEngine;

/// <summary>
/// Abstract base class for all weapons.
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] protected string _weaponName = "Weapon";
    [SerializeField] protected float _damage = 10f;
    [SerializeField] protected float _attackCooldown = 0.5f;
    [SerializeField] protected float _range = 1f;
    [SerializeField] protected float _knockbackForce = 2f;
    
    [Header("References")]
    [SerializeField] protected Transform _attackPoint;
    [SerializeField] protected LayerMask _targetLayers;
    
    // Properties
    public string WeaponName => _weaponName;
    public float Damage => _damage;
    public float Range => _range;
    public bool CanAttack => _cooldownTimer <= 0;
    
    protected float _cooldownTimer;
    protected bool _isAttacking;
    
    protected virtual void Update()
    {
        // Update cooldown
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Attempt to perform an attack.
    /// </summary>
    public virtual bool Attack()
    {
        if (!CanAttack) return false;
        
        _cooldownTimer = _attackCooldown;
        _isAttacking = true;
        
        PerformAttack();
        
        return true;
    }
    
    /// <summary>
    /// Override in derived classes to implement specific attack behavior.
    /// </summary>
    protected abstract void PerformAttack();
    
    /// <summary>
    /// Get the attack point position (defaults to weapon position).
    /// </summary>
    protected Vector2 GetAttackPoint()
    {
        return _attackPoint != null ? _attackPoint.position : transform.position;
    }
    
    /// <summary>
    /// Create damage info for this weapon's attack.
    /// </summary>
    protected DamageInfo CreateDamageInfo(Vector2 hitPoint, Vector2 direction)
    {
        return new DamageInfo(_damage, gameObject, DamageType.Physical)
            .WithKnockback(direction.normalized * _knockbackForce)
            .WithHitPoint(hitPoint);
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Vector3 point = _attackPoint != null ? _attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(point, _range);
    }
}
