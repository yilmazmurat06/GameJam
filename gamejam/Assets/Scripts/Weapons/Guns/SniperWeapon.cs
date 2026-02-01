using UnityEngine;

/// <summary>
/// Sniper weapon - SoulKnight style high damage precision weapon.
/// Slow fire rate, high damage, perfect accuracy.
/// Based on SoulKnight's sniper rifle patterns.
/// </summary>
public class SniperWeapon : RangedWeapon
{
    [Header("Sniper Specific")]
    [SerializeField] private float _chargeTime = 0.5f;
    [SerializeField] private float _penetrationCount = 2; // Hits through enemies
    [SerializeField] private bool _showLaserSight = true;
    [SerializeField] private LineRenderer _laserSight;
    
    private bool _isCharging;
    private float _chargeTimer;
    
    public bool IsCharging => _isCharging;
    public float ChargeProgress => Mathf.Clamp01(_chargeTimer / _chargeTime);
    
    protected virtual void Awake()
    {
        // SoulKnight Sniper stats
        _weaponName = "Sniper Rifle";
        _damage = 25f;          // Very high damage
        _attackCooldown = 1.5f; // Very slow fire rate
        _energyCost = 15f;      // High energy cost
        _precision = 1.0f;      // Perfect accuracy
        _attackSpeed = 1f;
        _projectileSpeed = 25f; // Very fast projectile
        _range = 15f;           // Long range
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Update laser sight
        if (_showLaserSight && _laserSight != null)
        {
            UpdateLaserSight();
        }
        
        // Handle charging
        if (_isCharging)
        {
            _chargeTimer += Time.deltaTime;
            
            // Check if charge is complete and fire
            if (_chargeTimer >= _chargeTime)
            {
                FireChargedShot();
            }
        }
    }
    
    public override bool Attack()
    {
        if (!CanAttack || !HasAmmo) return false;
        
        // Start charging
        _isCharging = true;
        _chargeTimer = 0;
        
        Debug.Log($"[SniperWeapon] Charging...");
        return true;
    }
    
    /// <summary>
    /// Cancel the charge (if player releases fire button).
    /// </summary>
    public void CancelCharge()
    {
        _isCharging = false;
        _chargeTimer = 0;
    }
    
    private void FireChargedShot()
    {
        _isCharging = false;
        _chargeTimer = 0;
        _cooldownTimer = _attackCooldown / _attackSpeed;
        
        if (_projectilePrefab == null)
        {
            Debug.LogWarning($"[SniperWeapon] No projectile prefab assigned");
            return;
        }
        
        Vector2 firePos = _firePoint != null ? _firePoint.position : GetAttackPoint();
        Vector2 direction = GetBaseAimDirection(); // Perfect accuracy, no spread
        
        GameObject projectileObj = Instantiate(_projectilePrefab, firePos, Quaternion.identity);
        
        // Rotate to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectileObj.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            // Sniper bullets can penetrate
            projectile.Initialize(_damage, direction, _projectileSpeed, _targetLayers, gameObject, _knockbackForce);
            projectile.SetPenetration((int)_penetrationCount);
        }
        
        Debug.Log($"[SniperWeapon] BANG! Fired charged shot!");
    }
    
    private void UpdateLaserSight()
    {
        if (_laserSight == null) return;
        
        Vector2 startPos = _firePoint != null ? _firePoint.position : transform.position;
        Vector2 direction = GetBaseAimDirection();
        
        // Raycast to find endpoint
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, _range, _targetLayers);
        Vector2 endPos = hit.collider != null ? hit.point : startPos + direction * _range;
        
        _laserSight.positionCount = 2;
        _laserSight.SetPosition(0, startPos);
        _laserSight.SetPosition(1, endPos);
        
        // Pulse effect when charging
        if (_isCharging)
        {
            float pulse = Mathf.PingPong(Time.time * 5f, 1f);
            Color laserColor = Color.Lerp(Color.red, Color.white, pulse);
            _laserSight.startColor = laserColor;
            _laserSight.endColor = laserColor * 0.5f;
        }
    }

    protected override void PerformAttack()
    {
        // Override to prevent default single shot behavior
        // Charging is handled separately
    }
}
