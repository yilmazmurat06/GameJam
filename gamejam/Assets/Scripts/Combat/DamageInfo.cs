using UnityEngine;

/// <summary>
/// Contains information about a damage instance.
/// </summary>
[System.Serializable]
public struct DamageInfo
{
    /// <summary>
    /// Amount of damage to deal.
    /// </summary>
    public float Amount;
    
    /// <summary>
    /// The GameObject that caused the damage.
    /// </summary>
    public GameObject Source;
    
    /// <summary>
    /// Type of damage (for resistance calculations).
    /// </summary>
    public DamageType Type;
    
    /// <summary>
    /// Knockback force to apply.
    /// </summary>
    public Vector2 KnockbackForce;
    
    /// <summary>
    /// Position where the damage originated (for directional effects).
    /// </summary>
    public Vector2 HitPoint;
    
    public DamageInfo(float amount, GameObject source = null, DamageType type = DamageType.Physical)
    {
        Amount = amount;
        Source = source;
        Type = type;
        KnockbackForce = Vector2.zero;
        HitPoint = Vector2.zero;
    }
    
    public DamageInfo WithKnockback(Vector2 force)
    {
        KnockbackForce = force;
        return this;
    }
    
    public DamageInfo WithHitPoint(Vector2 point)
    {
        HitPoint = point;
        return this;
    }
}

/// <summary>
/// Types of damage for resistance/weakness calculations.
/// </summary>
public enum DamageType
{
    Physical,
    Fire,
    Ice,
    Poison,
    Psychic  // For mask-based damage
}
