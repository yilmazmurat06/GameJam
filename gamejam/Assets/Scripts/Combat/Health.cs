using UnityEngine;
using System;

/// <summary>
/// Reusable health component for any entity.
/// Attach to players, enemies, or destructible objects.
/// Now includes SoulKnight-style armor system.
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth;
    
    [Header("Armor Settings")]
    [SerializeField] private float _maxArmor = 3f;
    [SerializeField] private float _currentArmor;
    [SerializeField] private float _armorRegenRate = 0.5f; // Armor per second
    [SerializeField] private float _armorRegenDelay = 3f; // Seconds after taking damage
    private float _armorRegenTimer;
    
    [Header("Invincibility")]
    [SerializeField] private float _invincibilityDuration = 0.5f;
    private float _invincibilityTimer;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool _flashOnDamage = true;
    [SerializeField] private Color _damageFlashColor = Color.red;
    [SerializeField] private float _flashDuration = 0.1f;
    
    // Events
    public event Action<DamageInfo> OnDamaged;
    public event Action<float> OnHealed;
    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action<float, float> OnArmorChanged; // current, max
    
    // Properties
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public float CurrentArmor => _currentArmor;
    public float MaxArmor => _maxArmor;
    public bool IsAlive => _currentHealth > 0;
    public float HealthPercent => _currentHealth / _maxHealth;
    public float ArmorPercent => _maxArmor > 0 ? _currentArmor / _maxArmor : 0;
    public bool IsInvincible => _invincibilityTimer > 0;
    public bool HasArmor => _currentArmor > 0;
    
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    
    private void Awake()
    {
        _currentHealth = _maxHealth;
        _currentArmor = _maxArmor;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
    }
    
    private void Update()
    {
        // Update invincibility timer
        if (_invincibilityTimer > 0)
        {
            _invincibilityTimer -= Time.deltaTime;
        }
        
        // Update armor regeneration
        if (_armorRegenTimer > 0)
        {
            _armorRegenTimer -= Time.deltaTime;
        }
        else if (_currentArmor < _maxArmor)
        {
            // Regenerate armor
            _currentArmor = Mathf.Min(_currentArmor + _armorRegenRate * Time.deltaTime, _maxArmor);
            OnArmorChanged?.Invoke(_currentArmor, _maxArmor);
        }
    }
    
    /// <summary>
    /// Apply damage to this entity.
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (!IsAlive) return;
        if (IsInvincible) return;
        
        // Apply damage multiplier (for Hate mask damage reduction)
        float actualDamage = damageInfo.Amount * _damageMultiplier;
        
        // Reset armor regen timer on any damage
        _armorRegenTimer = _armorRegenDelay;
        
        // Armor absorbs damage first (SoulKnight style)
        float damageToArmor = Mathf.Min(actualDamage, _currentArmor);
        float damageToHealth = actualDamage - damageToArmor;
        
        if (damageToArmor > 0)
        {
            _currentArmor -= damageToArmor;
            OnArmorChanged?.Invoke(_currentArmor, _maxArmor);
            Debug.Log($"[Health] Armor absorbed {damageToArmor} damage. Armor: {_currentArmor}/{_maxArmor}");
        }
        
        if (damageToHealth > 0)
        {
            _currentHealth -= damageToHealth;
            _currentHealth = Mathf.Max(0, _currentHealth);
        }
        
        // Start invincibility
        _invincibilityTimer = _invincibilityDuration;
        
        // Visual feedback
        if (_flashOnDamage && _spriteRenderer != null)
        {
            StartCoroutine(FlashRoutine());
        }
        
        // Apply knockback if we have a Rigidbody2D
        if (damageInfo.KnockbackForce != Vector2.zero)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(damageInfo.KnockbackForce, ForceMode2D.Impulse);
            }
        }
        
        // Fire events
        OnDamaged?.Invoke(damageInfo);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        
        Debug.Log($"[Health] {gameObject.name} took {actualDamage} damage. Health: {_currentHealth}/{_maxHealth}");
        
        // Check for death
        if (!IsAlive)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Heal this entity.
    /// </summary>
    public void Heal(float amount)
    {
        if (!IsAlive) return;
        
        float previousHealth = _currentHealth;
        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        float actualHeal = _currentHealth - previousHealth;
        
        if (actualHeal > 0)
        {
            OnHealed?.Invoke(actualHeal);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            Debug.Log($"[Health] {gameObject.name} healed {actualHeal}. Health: {_currentHealth}/{_maxHealth}");
        }
    }
    
    /// <summary>
    /// Set health to a specific value.
    /// </summary>
    public void SetHealth(float value)
    {
        _currentHealth = Mathf.Clamp(value, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        
        if (!IsAlive)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Reset health to maximum.
    /// </summary>
    public void ResetHealth()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
    
    /// <summary>
    /// Set maximum health (for mask effects like Fear).
    /// </summary>
    public void SetMaxHealth(float value)
    {
        _maxHealth = value;
        _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
    
    /// <summary>
    /// Set damage multiplier (for mask effects like Hate).
    /// </summary>
    public void SetDamageMultiplier(float multiplier)
    {
        _damageMultiplier = multiplier;
    }
    
    private float _damageMultiplier = 1f;
    
    private void Die()
    {
        Debug.Log($"[Health] {gameObject.name} died!");
        OnDeath?.Invoke();
    }
    
    private System.Collections.IEnumerator FlashRoutine()
    {
        _spriteRenderer.color = _damageFlashColor;
        yield return new WaitForSeconds(_flashDuration);
        _spriteRenderer.color = _originalColor;
    }
}
