using UnityEngine;
using System;

/// <summary>
/// Reusable health component for any entity.
/// Attach to players, enemies, or destructible objects.
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth;
    
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
    
    // Properties
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public bool IsAlive => _currentHealth > 0;
    public float HealthPercent => _currentHealth / _maxHealth;
    public bool IsInvincible => _invincibilityTimer > 0;
    
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    
    private void Awake()
    {
        _currentHealth = _maxHealth;
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
    }
    
    /// <summary>
    /// Apply damage to this entity.
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (!IsAlive) return;
        if (IsInvincible) return;
        
        _currentHealth -= damageInfo.Amount;
        _currentHealth = Mathf.Max(0, _currentHealth);
        
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
        
        Debug.Log($"[Health] {gameObject.name} took {damageInfo.Amount} damage. Health: {_currentHealth}/{_maxHealth}");
        
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
