using UnityEngine;

/// <summary>
/// Enemy attack state - attacks the player when in range.
/// </summary>
public class EnemyAttackState : IEnemyState
{
    private float _timer;
    private bool _hasAttacked;
    private AttackPhase _phase;
    
    private enum AttackPhase { WindUp, Recovery }
    
    public void Enter(EnemyBase enemy)
    {
        // Start WindUp
        _phase = AttackPhase.WindUp;
        _timer = enemy.AttackWindUpTime;
        _hasAttacked = false;
        
        enemy.SetVelocity(Vector2.zero);
        
        Debug.Log($"[{enemy.EnemyName}] Winding up attack...");
    }
    
    public void Execute(EnemyBase enemy)
    {
        if (enemy.Target == null)
        {
            enemy.ChangeState(new EnemyIdleState());
            return;
        }

        _timer -= Time.deltaTime;
        
        if (_phase == AttackPhase.WindUp)
        {
            // Face target during windup
            Vector2 dir = enemy.Target.position - enemy.transform.position;
            if (dir.x != 0 && enemy.GetComponent<SpriteRenderer>() is SpriteRenderer sr)
                sr.flipX = dir.x < 0;

            if (_timer <= 0)
            {
                // Perform Attack
                enemy.Attack();
                
                // Switch to Recovery
                _phase = AttackPhase.Recovery;
                _timer = enemy.AttackRecoveryTime;
                Debug.Log($"[{enemy.EnemyName}] Recovering...");
            }
        }
        else if (_phase == AttackPhase.Recovery)
        {
            if (_timer <= 0)
            {
                // Finished attack cycle
                // If player still in range, maybe chase or attack again? 
                // Let's go back to Chase to decide (Chase will attack if in range)
                enemy.ChangeState(new EnemyChaseState());
            }
        }
    }
    
    public void Exit(EnemyBase enemy)
    {
        _hasAttacked = false;
    }
}
