using UnityEngine;

/// <summary>
/// SMG weapon - SoulKnight style rapid fire weapon.
/// High fire rate, low damage, moderate spread.
/// Based on SoulKnight's SMG patterns.
/// </summary>
public class SMGWeapon : RangedWeapon
{
    [Header("SMG Specific")]
    [SerializeField] private int _burstCount = 3;
    [SerializeField] private float _burstDelay = 0.08f;
    [SerializeField] private float _recoilPerShot = 0.5f;
    
    private int _currentBurst;
    private float _burstTimer;
    private float _currentRecoil;
    
    protected virtual void Awake()
    {
        // SoulKnight SMG stats
        _weaponName = "SMG";
        _damage = 3f;           // Low damage per shot
        _attackCooldown = 0.4f; // Between bursts
        _energyCost = 4f;       // Low energy per burst
        _precision = 0.75f;     // Moderate accuracy
        _attackSpeed = 1.2f;    // Fast
        _projectileSpeed = 12f;
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Handle burst firing
        if (_currentBurst > 0)
        {
            _burstTimer -= Time.deltaTime;
            if (_burstTimer <= 0)
            {
                FireBurstShot();
                _currentBurst--;
                _burstTimer = _burstDelay;
            }
        }
        
        // Decay recoil
        _currentRecoil = Mathf.Max(0, _currentRecoil - Time.deltaTime * 5f);
    }
    
    public override bool Attack()
    {
        if (!CanAttack || !HasAmmo) return false;
        
        // Start burst
        _currentBurst = _burstCount;
        _burstTimer = 0; // Fire first shot immediately
        
        _cooldownTimer = _attackCooldown / _attackSpeed;
        return true;
    }
    
    private void FireBurstShot()
    {
        if (_projectilePrefab == null) return;
        
        Vector2 firePos = _firePoint != null ? _firePoint.position : GetAttackPoint();
        Vector2 direction = GetAimDirectionWithRecoil();
        
        GameObject projectileObj = Instantiate(_projectilePrefab, firePos, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            projectile.Initialize(_damage, direction, _projectileSpeed, _targetLayers, gameObject, _knockbackForce);
        }
        
        // Add recoil
        _currentRecoil += _recoilPerShot;
        
        // Consume ammo
        if (_maxAmmo > 0)
        {
            // Handled per burst, not per shot for balance
        }
    }
    
    private Vector2 GetAimDirectionWithRecoil()
    {
        Vector2 baseDir = GetBaseAimDirection();
        
        // Apply spread based on precision AND current recoil
        float maxSpread = (1f - _precision) * 15f + _currentRecoil;
        float spreadAngle = Random.Range(-maxSpread, maxSpread);
        
        return Quaternion.Euler(0, 0, spreadAngle) * baseDir;
    }

    protected override void PerformAttack()
    {
        // Override to prevent default single shot behavior
        // Burst is handled in Update
    }
}
