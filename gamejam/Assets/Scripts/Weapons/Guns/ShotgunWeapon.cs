using UnityEngine;

/// <summary>
/// Shotgun weapon - SoulKnight style spread weapon.
/// Multiple projectiles in a spread pattern.
/// Based on SoulKnight's shotgun patterns.
/// </summary>
public class ShotgunWeapon : RangedWeapon
{
    [Header("Shotgun Specific")]
    [SerializeField] private int _pelletCount = 5;
    [SerializeField] private float _spreadAngle = 30f;
    [SerializeField] private float _pushbackForce = 3f; // Self-knockback
    
    protected virtual void Awake()
    {
        // SoulKnight Shotgun stats
        _weaponName = "Shotgun";
        _damage = 4f;           // Damage per pellet
        _attackCooldown = 0.8f; // Slow fire rate
        _energyCost = 8f;       // High energy cost
        _precision = 0.6f;      // Each pellet has spread
        _attackSpeed = 1f;      
        _projectileSpeed = 10f;
        _range = 4f;            // Short effective range
    }
    
    protected override void PerformAttack()
    {
        if (_projectilePrefab == null)
        {
            Debug.LogWarning($"[ShotgunWeapon] No projectile prefab assigned");
            return;
        }
        
        Vector2 firePos = _firePoint != null ? _firePoint.position : GetAttackPoint();
        Vector2 baseDirection = GetBaseAimDirection();
        
        // Fire multiple pellets in a spread pattern
        float angleStep = _spreadAngle / (_pelletCount - 1);
        float startAngle = -_spreadAngle / 2f;
        
        for (int i = 0; i < _pelletCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            
            // Add random variation to each pellet
            float randomOffset = Random.Range(-5f, 5f) * (1f - _precision);
            angle += randomOffset;
            
            Vector2 direction = Quaternion.Euler(0, 0, angle) * baseDirection;
            
            SpawnPellet(firePos, direction);
        }
        
        // Apply self-knockback (SoulKnight style recoil)
        ApplySelfKnockback(-baseDirection);
        
        Debug.Log($"[ShotgunWeapon] Fired {_pelletCount} pellets!");
    }
    
    private void SpawnPellet(Vector2 position, Vector2 direction)
    {
        GameObject projectileObj = Instantiate(_projectilePrefab, position, Quaternion.identity);
        
        // Rotate to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectileObj.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            // Pellets have shorter range and reduced knockback
            projectile.Initialize(_damage, direction, _projectileSpeed, _targetLayers, gameObject, _knockbackForce * 0.5f);
        }
    }
    
    private void ApplySelfKnockback(Vector2 direction)
    {
        // Find player rigidbody
        Rigidbody2D playerRb = GetComponentInParent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.AddForce(direction * _pushbackForce, ForceMode2D.Impulse);
        }
    }
}
