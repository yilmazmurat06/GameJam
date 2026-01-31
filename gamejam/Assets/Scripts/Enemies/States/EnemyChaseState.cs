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
        
        // In attack range?
        if (enemy.IsTargetInAttackRange())
        {
            // Try to get token. If yes, Attack. If no, Strafe.
            if (enemy.CanAttack && EnemyManager.Instance.RequestAttackToken(enemy))
            {
                enemy.ChangeState(new EnemyAttackState());
            }
            else
            {
                enemy.ChangeState(new EnemyStrafeState());
            }
            return;
        }
        
        // Smooth Move toward target with Separation
        Vector2 targetPos = enemy.Target.position;
        Vector2 separation = EnemyManager.Instance.GetSeparationVector(enemy, 1.2f); // Avoid others
        
        // Blend movement and separation
        enemy.SmoothMoveToward(targetPos + separation); // Boid-like steering
    }
    
    public void Exit(EnemyBase enemy)
    {
        enemy.SetVelocity(Vector2.zero);
    }
}
