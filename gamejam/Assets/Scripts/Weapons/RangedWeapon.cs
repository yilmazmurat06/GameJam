using UnityEngine;

/// <summary>
/// Ranged weapon that fires projectiles.
/// Now uses mouse aiming and precision-based spread.
/// </summary>
public class RangedWeapon : WeaponBase
{
    [Header("Ranged Settings")]
    [SerializeField] protected GameObject _projectilePrefab;
    [SerializeField] protected float _projectileSpeed = 10f;
    [SerializeField] protected int _maxAmmo = -1; // -1 = infinite
    [SerializeField] protected Transform _firePoint;
    
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
    /// Get aim direction with precision-based spread.
    /// Uses mouse position for aiming.
    /// </summary>
    protected virtual Vector2 GetAimDirection()
    {
        Vector2 baseDir = GetBaseAimDirection();
        
        // Apply spread based on precision (lower precision = more spread)
        float maxSpread = (1f - _precision) * 15f; // Up to 15 degrees spread at 0 precision
        float spreadAngle = Random.Range(-maxSpread, maxSpread);
        
        return Quaternion.Euler(0, 0, spreadAngle) * baseDir;
    }
    
    /// <summary>
    /// Get base aim direction before spread is applied.
    /// </summary>
    protected virtual Vector2 GetBaseAimDirection()
    {
        // Try to get aim direction from player input handler
        PlayerInputHandler inputHandler = GetComponentInParent<PlayerInputHandler>();
        if (inputHandler != null)
        {
            return inputHandler.AimDirection;
        }
        
        // Fallback: fire in facing direction
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
