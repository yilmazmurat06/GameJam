using UnityEngine;

/// <summary>
/// Basic pistol weapon - balanced starter weapon.
/// High precision, low damage, moderate fire rate.
/// </summary>
public class Pistol : RangedWeapon
{
    private void Reset()
    {
        // Set default values for Pistol when component is added
        _weaponName = "Pistol";
        _damage = 10f;
        _attackCooldown = 0.4f;
        _range = 10f;
        _knockbackForce = 2f;
        _energyCost = 2f;
        _precision = 0.95f; // Very accurate
        _attackSpeed = 1f;
    }
}
