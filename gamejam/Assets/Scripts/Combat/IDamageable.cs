/// <summary>
/// Interface for any entity that can receive damage.
/// Implement on players, enemies, destructible objects, etc.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Apply damage to this entity.
    /// </summary>
    void TakeDamage(DamageInfo damageInfo);
    
    /// <summary>
    /// Current health of the entity.
    /// </summary>
    float CurrentHealth { get; }
    
    /// <summary>
    /// Maximum health of the entity.
    /// </summary>
    float MaxHealth { get; }
    
    /// <summary>
    /// Whether this entity is still alive.
    /// </summary>
    bool IsAlive { get; }
}
