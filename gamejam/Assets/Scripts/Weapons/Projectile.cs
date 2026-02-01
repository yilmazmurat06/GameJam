using UnityEngine;

/// <summary>
/// Projectile that moves in a direction and damages on impact.
/// SoulKnight-style with penetration support.
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
    
    // SoulKnight-style penetration
    private int _penetrationCount = 0;
    private int _currentPenetrations = 0;
    
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
        _rigidbody.linearVelocity = _direction * _speed;
        
        // Rotate to face direction
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Destroy after lifetime
        Destroy(gameObject, _lifetime);
    }
    
    /// <summary>
    /// Alternative initialize for enemy projectiles.
    /// </summary>
    public void Initialize(GameObject source, int damage, Vector2 direction)
    {
        _damage = damage;
        _direction = direction.normalized;
        _source = source;
        _speed = 8f; // Default enemy projectile speed
        _knockbackForce = 2f;
        _initialized = true;
        
        // Set velocity
        _rigidbody.linearVelocity = _direction * _speed;
        
        // Rotate to face direction
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Set layer to enemy projectiles
        _targetLayers = LayerMask.GetMask("Player");
        
        // Destroy after lifetime
        Destroy(gameObject, _lifetime);
    }
    
    /// <summary>
    /// Set penetration count (SoulKnight sniper-style).
    /// </summary>
    public void SetPenetration(int count)
    {
        _penetrationCount = count;
        _destroyOnHit = count <= 0;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_initialized) return;
        
        // Skip source (with null check in case source was destroyed)
        if (_source != null && (other.gameObject == _source || other.transform.IsChildOf(_source.transform)))
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
        
        // Handle penetration (SoulKnight style)
        if (_penetrationCount > 0)
        {
            _currentPenetrations++;
            if (_currentPenetrations >= _penetrationCount)
            {
                Destroy(gameObject);
            }
            // Else continue through
        }
        else if (_destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
