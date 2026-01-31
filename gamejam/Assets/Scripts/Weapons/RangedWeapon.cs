using UnityEngine;

/// <summary>
/// Ranged weapon that fires projectiles.
/// </summary>
public class RangedWeapon : WeaponBase
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileSpeed = 10f;
    [SerializeField] private int _maxAmmo = -1; // -1 = infinite
    [SerializeField] private Transform _firePoint;
    
    private int _currentAmmo;
    
    public int CurrentAmmo => _currentAmmo;
    public bool HasAmmo => _maxAmmo < 0 || _currentAmmo > 0;
    
    private void Start()
    {
        _currentAmmo = _maxAmmo;
    }
    
    public override bool Attack()
    {
        if (!HasAmmo) return false;
        return base.Attack();
    }
    
    protected override void PerformAttack()
    {
        if (_projectilePrefab == null)
        {
            Debug.LogWarning($"[RangedWeapon] No projectile prefab assigned to {_weaponName}");
            return;
        }
        
        // Get fire direction (based on facing direction or mouse position)
        Vector2 firePos = _firePoint != null ? _firePoint.position : GetAttackPoint();
        Vector2 direction = GetAimDirection();
        
        // Spawn projectile
        GameObject projectileObj = Instantiate(_projectilePrefab, firePos, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            projectile.Initialize(_damage, direction, _projectileSpeed, _targetLayers, gameObject, _knockbackForce);
        }
        
        // Consume ammo
        if (_maxAmmo > 0)
        {
            _currentAmmo--;
        }
        
        Debug.Log($"[RangedWeapon] {_weaponName} fired projectile");
    }
    
    /// <summary>
    /// Get aim direction (override for mouse aiming).
    /// </summary>
    protected virtual Vector2 GetAimDirection()
    {
        // Default: fire in facing direction
        SpriteRenderer sr = GetComponentInParent<SpriteRenderer>();
        if (sr != null)
        {
            return sr.flipX ? Vector2.left : Vector2.right;
        }
        return Vector2.right;
    }
    
    /// <summary>
    /// Add ammo to the weapon.
    /// </summary>
    public void AddAmmo(int amount)
    {
        if (_maxAmmo > 0)
        {
            _currentAmmo = Mathf.Min(_currentAmmo + amount, _maxAmmo);
        }
    }
}
