using UnityEngine;

/// <summary>
/// Melee weapon that damages enemies within an overlap circle.
/// </summary>
public class MeleeWeapon : WeaponBase
{
    [Header("Melee Settings")]
    [SerializeField] private float _attackAngle = 90f; // For arc attacks (future)
    
    protected override void PerformAttack()
    {
        Vector2 attackPos = GetAttackPoint();
        
        // Find all colliders in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, _range, _targetLayers);
        
        foreach (Collider2D hit in hits)
        {
            // Skip self
            if (hit.gameObject == gameObject || hit.transform.IsChildOf(transform.root))
                continue;
            
            // Try to damage
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 direction = (hit.transform.position - transform.position).normalized;
                DamageInfo damageInfo = CreateDamageInfo(hit.ClosestPoint(attackPos), direction);
                
                damageable.TakeDamage(damageInfo);
                Debug.Log($"[MeleeWeapon] {_weaponName} hit {hit.name} for {_damage} damage");
            }
        }
        
        // Trigger attack animation if available
        Animator animator = GetComponentInParent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
}
