using UnityEngine;
using System.Collections;

/// <summary>
/// Sword weapon - SoulKnight style melee weapon with swing arc.
/// Fast attacks, moderate damage, can hit multiple enemies.
/// Based on SoulKnight's blade weapons.
/// </summary>
public class SwordWeapon : MeleeWeapon
{
    [Header("Sword Specific")]
    [SerializeField] private float _swingArc = 120f;       // Degrees of swing
    [SerializeField] private float _swingDuration = 0.2f;  // Time to complete swing
    [SerializeField] private int _comboMaxHits = 3;        // Max combo hits
    [SerializeField] private float _comboWindowTime = 0.5f; // Time to continue combo
    [SerializeField] private float _comboDamageMultiplier = 1.2f; // Damage increase per combo
    [SerializeField] private TrailRenderer _swingTrail;
    
    private int _currentCombo;
    private float _comboTimer;
    private bool _isSwinging;
    
    public int CurrentCombo => _currentCombo;
    public bool IsSwinging => _isSwinging;
    
    protected virtual void Awake()
    {
        // SoulKnight Sword stats
        _weaponName = "Sword";
        _damage = 12f;          // Good base damage
        _attackCooldown = 0.35f; // Fast attacks
        _energyCost = 0f;       // Melee uses no energy
        _precision = 1f;        // Always hits (melee)
        _attackSpeed = 1.2f;
        _range = 1.5f;          // Melee range
        _knockbackForce = 3f;
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Update combo timer
        if (_comboTimer > 0)
        {
            _comboTimer -= Time.deltaTime;
            if (_comboTimer <= 0)
            {
                _currentCombo = 0; // Reset combo
            }
        }
    }
    
    public override bool Attack()
    {
        if (!CanAttack || _isSwinging) return false;
        
        // Increment combo
        _currentCombo = Mathf.Min(_currentCombo + 1, _comboMaxHits);
        _comboTimer = _comboWindowTime;
        
        // Start swing coroutine
        StartCoroutine(PerformSwing());
        
        _cooldownTimer = _attackCooldown / _attackSpeed;
        return true;
    }
    
    private IEnumerator PerformSwing()
    {
        _isSwinging = true;
        
        // Enable trail
        if (_swingTrail != null)
            _swingTrail.emitting = true;
        
        // Calculate combo damage
        float comboDamage = _damage * Mathf.Pow(_comboDamageMultiplier, _currentCombo - 1);
        
        // Get swing direction based on facing
        Vector2 baseDirection = GetSwingDirection();
        float startAngle = -_swingArc / 2f;
        float endAngle = _swingArc / 2f;
        
        // Alternate swing direction for combo
        if (_currentCombo % 2 == 0)
        {
            float temp = startAngle;
            startAngle = endAngle;
            endAngle = temp;
        }
        
        // Track already hit targets to prevent multi-hit
        System.Collections.Generic.HashSet<Collider2D> hitTargets = new();
        
        float elapsed = 0f;
        while (elapsed < _swingDuration)
        {
            float t = elapsed / _swingDuration;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            
            // Rotate attack direction
            Vector2 attackDir = Quaternion.Euler(0, 0, currentAngle) * baseDirection;
            Vector2 attackPos = (Vector2)transform.position + attackDir * _range * 0.5f;
            
            // Check for hits at current swing position
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, _range * 0.5f, _targetLayers);
            
            foreach (Collider2D hit in hits)
            {
                if (hitTargets.Contains(hit)) continue;
                if (hit.gameObject == gameObject || hit.transform.IsChildOf(transform.root)) continue;
                
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Vector2 direction = (hit.transform.position - transform.position).normalized;
                    DamageInfo damageInfo = new DamageInfo(comboDamage, gameObject, DamageType.Physical)
                        .WithKnockback(direction * _knockbackForce)
                        .WithHitPoint(hit.ClosestPoint(attackPos));
                    
                    damageable.TakeDamage(damageInfo);
                    hitTargets.Add(hit);
                    
                    Debug.Log($"[SwordWeapon] Combo {_currentCombo} hit {hit.name} for {comboDamage} damage!");
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Disable trail
        if (_swingTrail != null)
            _swingTrail.emitting = false;
        
        _isSwinging = false;
    }
    
    private Vector2 GetSwingDirection()
    {
        // Try to get aim direction from player
        PlayerInputHandler inputHandler = GetComponentInParent<PlayerInputHandler>();
        if (inputHandler != null)
        {
            return inputHandler.AimDirection;
        }
        
        // Fallback: use facing direction
        SpriteRenderer sr = GetComponentInParent<SpriteRenderer>();
        if (sr != null)
        {
            return sr.flipX ? Vector2.left : Vector2.right;
        }
        return Vector2.right;
    }

    protected override void PerformAttack()
    {
        // Override - swing is handled by coroutine
    }
}
