using UnityEngine;

/// <summary>
/// Projectile that moves in a direction and damages on impact.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float _lifetime = 5f;
    [SerializeField] private bool _destroyOnHit = true;
    [SerializeField] private GameObject _hitEffectPrefab;
    
    private float _damage;
    private float _speed;
    private float _knockbackForce;
    private Vector2 _direction;
    private LayerMask _targetLayers;
    private GameObject _source;
    private Rigidbody2D _rigidbody;
    private bool _initialized;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0f;
    }
    
    /// <summary>
    /// Initialize the projectile with its parameters.
    /// </summary>
    public void Initialize(float damage, Vector2 direction, float speed, LayerMask targetLayers, GameObject source, float knockback = 0f)
    {
        _damage = damage;
        _direction = direction.normalized;
        _speed = speed;
        _targetLayers = targetLayers;
        _source = source;
        _knockbackForce = knockback;
        _initialized = true;
        
        // Set velocity
        _rigidbody.velocity = _direction * _speed;
        
        // Rotate to face direction
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Destroy after lifetime
        Destroy(gameObject, _lifetime);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_initialized) return;
        
        // Skip source
        if (other.gameObject == _source || other.transform.IsChildOf(_source.transform))
            return;
        
        // Check if target is on target layer
        if ((_targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;
        
        // Try to damage
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            DamageInfo damageInfo = new DamageInfo(_damage, _source, DamageType.Physical)
                .WithKnockback(_direction * _knockbackForce)
                .WithHitPoint(transform.position);
            
            damageable.TakeDamage(damageInfo);
            Debug.Log($"[Projectile] Hit {other.name} for {_damage} damage");
        }
        
        // Spawn hit effect
        if (_hitEffectPrefab != null)
        {
            Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Destroy if configured
        if (_destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
