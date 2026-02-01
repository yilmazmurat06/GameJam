using UnityEngine;

/// <summary>
/// SMG weapon - rapid fire, low accuracy.
/// Low damage, very fast fire rate, high energy consumption.
/// </summary>
public class SMG : RangedWeapon
{
    private void Reset()
    {
        // Set default values for SMG when component is added
        _weaponName = "SMG";
        _damage = 6f;
        _attackCooldown = 0.1f;
        _range = 8f;
        _knockbackForce = 1f;
        _energyCost = 1f;
        _precision = 0.7f; // Low accuracy
        _attackSpeed = 2.5f;
    }
}
