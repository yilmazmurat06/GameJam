using UnityEngine;

/// <summary>
/// Player attack state - performs an attack with equipped weapon.
/// </summary>
public class PlayerAttackState : IPlayerState
{
    private float _attackDuration = 0.3f;
    private float _timer;
    
    public void Enter(PlayerController player)
    {
        _timer = _attackDuration;
        
        // Stop movement during attack
        player.SetVelocity(Vector2.zero);
        
        // Perform attack with equipped weapon
        if (player.CurrentWeapon != null)
        {
            player.CurrentWeapon.Attack();
        }
        else
        {
            // Unarmed attack (basic melee)
            PerformUnarmedAttack(player);
        }
        
        Debug.Log("[PlayerAttackState] Attacking!");
    }
    
    public void Execute(PlayerController player)
    {
        _timer -= Time.deltaTime;
        
        // Return to idle/move when attack finished
        if (_timer <= 0)
        {
            if (player.InputHandler.MoveInput.magnitude > 0.1f)
            {
                player.ChangeState(new PlayerMoveState());
            }
            else
            {
                player.ChangeState(new PlayerIdleState());
            }
        }
    }
    
    public void Exit(PlayerController player)
    {
        // Cleanup
    }
    
    private void PerformUnarmedAttack(PlayerController player)
    {
        // Basic melee attack without weapon
        float range = 1f;
        float damage = 5f;
        LayerMask enemyLayer = LayerMask.GetMask("Enemy");
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.transform.position, range, enemyLayer);
        
        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 direction = (hit.transform.position - player.transform.position).normalized;
                DamageInfo damageInfo = new DamageInfo(damage, player.gameObject, DamageType.Physical)
                    .WithKnockback(direction * 2f);
                
                damageable.TakeDamage(damageInfo);
            }
        }
    }
}
