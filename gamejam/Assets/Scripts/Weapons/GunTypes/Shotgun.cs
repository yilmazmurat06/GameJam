using UnityEngine;

/// <summary>
/// Shotgun weapon - fires multiple pellets in a spread pattern.
/// High damage potential, slow fire rate, wide spread.
/// </summary>
public class Shotgun : RangedWeapon
{
    [Header("Shotgun Settings")]
    [SerializeField] private int _pelletCount = 6;
    [SerializeField] private float _spreadAngle = 30f; // Total spread angle
    
    private void Reset()
    {
        // Set default values for Shotgun when component is added
        _weaponName = "Shotgun";
        _damage = 5f; // Per pellet
        _attackCooldown = 0.8f;
        _range = 6f;
        _knockbackForce = 4f;
        _energyCost = 8f;
        _precision = 0.5f;
        _attackSpeed = 0.6f;
        _pelletCount = 6;
        _spreadAngle = 30f;
    }
    
    protected override void PerformAttack()
    {
        if (_projectilePrefab == null)
        {
            Debug.LogWarning($"[Shotgun] No projectile prefab assigned to {_weaponName}");
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
            // Add some randomness to each pellet
            angle += Random.Range(-2f, 2f);
            
            Vector2 direction = Quaternion.Euler(0, 0, angle) * baseDirection;
            
            SpawnPellet(firePos, direction);
        }
        
        Debug.Log($"[Shotgun] {_weaponName} fired {_pelletCount} pellets");
    }
    
    private void SpawnPellet(Vector2 position, Vector2 direction)
    {
        GameObject projectileObj = Instantiate(_projectilePrefab, position, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            projectile.Initialize(_damage, direction, _projectileSpeed, _targetLayers, gameObject, _knockbackForce);
        }
    }
}

