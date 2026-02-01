using UnityEngine;

/// <summary>
/// Goblin enemy - SoulKnight style melee enemy.
/// Fast, aggressive, charges at player for melee attacks.
/// Based on SoulKnight's Goblin monster pattern.
/// </summary>
public class GoblinEnemy : EnemyBase
{
    [Header("Goblin Specific")]
    [SerializeField] private float _chargeSpeedMultiplier = 1.5f;
    [SerializeField] private float _chargeDistance = 3f;
    [SerializeField] private float _chargeCooldown = 3f;
    [SerializeField] private float _aggressiveness = 0.7f; // How likely to charge vs walk
    
    private float _chargeTimer;
    private bool _isCharging;
    private Vector2 _chargeDirection;
    
    public float ChargeSpeedMultiplier => _chargeSpeedMultiplier;
    public float ChargeDistance => _chargeDistance;
    public bool IsCharging => _isCharging;
    public bool CanCharge => _chargeTimer <= 0 && !_isCharging;
    
    protected override void Awake()
    {
        base.Awake();
        
        // SoulKnight Goblin stats
        _enemyName = "Goblin";
        _moveSpeed = 4f;           // Fast
        _attackDamage = 8f;        // Medium damage
        _attackRange = 1.2f;       // Melee range
        _attackCooldown = 0.8f;    // Fast attacks
        _detectionRange = 6f;      // Good detection
        _reactionTime = 0.3f;      // Quick reactions
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Update charge cooldown
        if (_chargeTimer > 0)
            _chargeTimer -= Time.deltaTime;
    }
    
    /// <summary>
    /// Start a charge attack toward the player.
    /// </summary>
    public void StartCharge()
    {
        if (!CanCharge || Target == null) return;
        
        _isCharging = true;
        _chargeDirection = GetDirectionToTarget();
        _chargeTimer = _chargeCooldown;
        
        Debug.Log($"[{EnemyName}] CHARGE!");
    }
    
    /// <summary>
    /// Execute charge movement.
    /// </summary>
    public void ExecuteCharge()
    {
        if (!_isCharging) return;
        
        // Move in charge direction at boosted speed
        SetVelocity(_chargeDirection * MoveSpeed * _chargeSpeedMultiplier);
    }
    
    /// <summary>
    /// End the charge.
    /// </summary>
    public void EndCharge()
    {
        _isCharging = false;
        SetVelocity(Vector2.zero);
    }
    
    /// <summary>
    /// Should this goblin charge? (AI decision helper)
    /// </summary>
    public bool ShouldCharge()
    {
        if (!CanCharge || Target == null) return false;
        
        float distance = GetDistanceToTarget();
        
        // Charge if in charge range but not too close
        if (distance <= _chargeDistance && distance > AttackRange)
        {
            return Random.value < _aggressiveness;
        }
        
        return false;
    }
    
    public override void Attack()
    {
        // End charge before attacking
        if (_isCharging)
            EndCharge();
        
        base.Attack();
        
        // Goblins have a small knockback on themselves after attacking (recoil)
        Vector2 recoil = -GetDirectionToTarget() * 0.5f;
        Rigidbody.AddForce(recoil, ForceMode2D.Impulse);
    }
}
