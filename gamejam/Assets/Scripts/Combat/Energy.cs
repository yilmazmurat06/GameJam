using UnityEngine;
using System;

/// <summary>
/// Energy (MP/Mana) system for weapon usage.
/// Weapons consume energy when fired, energy regenerates over time.
/// </summary>
public class Energy : MonoBehaviour
{
    [Header("Energy Settings")]
    [SerializeField] private float _maxEnergy = 100f;
    [SerializeField] private float _currentEnergy;
    
    [Header("Regeneration")]
    [SerializeField] private float _regenRate = 5f; // Energy per second
    [SerializeField] private float _regenDelay = 1f; // Delay after using energy before regen starts
    
    private float _regenTimer;
    
    // Events
    public event Action<float, float> OnEnergyChanged; // current, max
    public event Action OnEnergyDepleted;
    public event Action OnEnergyRestored; // When energy starts regenerating from 0
    
    // Properties
    public float CurrentEnergy => _currentEnergy;
    public float MaxEnergy => _maxEnergy;
    public float EnergyPercent => _currentEnergy / _maxEnergy;
    public bool HasEnergy => _currentEnergy > 0;
    
    private void Awake()
    {
        _currentEnergy = _maxEnergy;
    }
    
    private void Update()
    {
        // Handle regeneration
        if (_regenTimer > 0)
        {
            _regenTimer -= Time.deltaTime;
        }
        else if (_currentEnergy < _maxEnergy)
        {
            // Regenerate energy
            float previousEnergy = _currentEnergy;
            _currentEnergy = Mathf.Min(_currentEnergy + _regenRate * Time.deltaTime, _maxEnergy);
            
            // Check if we just restored from 0
            if (previousEnergy <= 0 && _currentEnergy > 0)
            {
                OnEnergyRestored?.Invoke();
            }
            
            OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
        }
    }
    
    /// <summary>
    /// Attempt to consume energy for an action (like firing a weapon).
    /// Returns true if successful, false if not enough energy.
    /// </summary>
    public bool TryConsume(float amount)
    {
        if (_currentEnergy < amount)
        {
            Debug.Log($"[Energy] Not enough energy. Need {amount}, have {_currentEnergy}");
            return false;
        }
        
        _currentEnergy -= amount;
        _regenTimer = _regenDelay;
        
        OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
        
        if (_currentEnergy <= 0)
        {
            _currentEnergy = 0;
            OnEnergyDepleted?.Invoke();
            Debug.Log("[Energy] Energy depleted!");
        }
        
        Debug.Log($"[Energy] Consumed {amount} energy, remaining: {_currentEnergy}/{_maxEnergy}");
        return true;
    }
    
    /// <summary>
    /// Force consume energy without checking (for special effects).
    /// </summary>
    public void Consume(float amount)
    {
        _currentEnergy = Mathf.Max(0, _currentEnergy - amount);
        _regenTimer = _regenDelay;
        OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
        
        if (_currentEnergy <= 0)
        {
            OnEnergyDepleted?.Invoke();
        }
    }
    
    /// <summary>
    /// Add energy instantly (from pickups, etc.).
    /// </summary>
    public void AddEnergy(float amount)
    {
        float previousEnergy = _currentEnergy;
        _currentEnergy = Mathf.Min(_currentEnergy + amount, _maxEnergy);
        
        if (previousEnergy <= 0 && _currentEnergy > 0)
        {
            OnEnergyRestored?.Invoke();
        }
        
        OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
        Debug.Log($"[Energy] Added {amount} energy. Current: {_currentEnergy}/{_maxEnergy}");
    }
    
    /// <summary>
    /// Reset energy to maximum.
    /// </summary>
    public void ResetEnergy()
    {
        _currentEnergy = _maxEnergy;
        _regenTimer = 0;
        OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
    }
    
    /// <summary>
    /// Set maximum energy (for upgrades/power-ups).
    /// </summary>
    public void SetMaxEnergy(float value)
    {
        _maxEnergy = value;
        _currentEnergy = Mathf.Min(_currentEnergy, _maxEnergy);
        OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
    }
    
    /// <summary>
    /// Check if there's enough energy for an action without consuming it.
    /// </summary>
    public bool CanAfford(float amount)
    {
        return _currentEnergy >= amount;
    }
}
