using UnityEngine;

/// <summary>
/// Enemy chase state - moves toward the player.
/// </summary>
public class EnemyChaseState : IEnemyState
{
    private float _reactionTimer;
    private bool _isChasing;

    public void Enter(EnemyBase enemy)
    {
        // Start "surprised" delay
        _reactionTimer = enemy.ReactionTime;
        _isChasing = false;
        enemy.SetVelocity(Vector2.zero);
        
        Debug.Log($"[{enemy.EnemyName}] Spotted player! Reacting...");
    }
    
    public void Execute(EnemyBase enemy)
    {
        // Lost target - return to idle
        if (!enemy.HasTarget)
        {
            enemy.ChangeState(new EnemyIdleState());
            return;
        }
        
        // Handle reaction delay
        if (!_isChasing)
        {
            _reactionTimer -= Time.deltaTime;
            if (_reactionTimer <= 0)
            {
                _isChasing = true;
                Debug.Log($"[{enemy.EnemyName}] Starting chase!");
            }
            return; // Don't move yet
        }
        
        // In attack range - attack
        if (enemy.IsTargetInAttackRange())
        {
            enemy.ChangeState(new EnemyAttackState());
            return;
        }
        
        // Smooth Move toward target with basic obstacle avoidance check
        Vector2 targetPos = enemy.Target.position;
        enemy.SmoothMoveToward(targetPos);
    }
    
    public void Exit(EnemyBase enemy)
    {
        enemy.SetVelocity(Vector2.zero);
    }
}
