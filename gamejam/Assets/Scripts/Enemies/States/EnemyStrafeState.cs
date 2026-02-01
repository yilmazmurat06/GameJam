using UnityEngine;

/// <summary>
/// Strafe/Circling state.
/// Used when the enemy wants to attack but cannot get a token.
/// Moves sideways relative to the target to "orbit" them.
/// </summary>
public class EnemyStrafeState : IEnemyState
{
    private float _timer;
    private float _strafeDuration;
    private int _direction; // 1 or -1
    
    public void Enter(EnemyBase enemy)
    {
        _strafeDuration = Random.Range(1f, 3f);
        _timer = _strafeDuration;
        _direction = Random.value > 0.5f ? 1 : -1;
        
        Debug.Log($"[{enemy.EnemyName}] Strafing around player...");
    }
    
    public void Execute(EnemyBase enemy)
    {
        if (enemy.Target == null)
        {
            enemy.ChangeState(new EnemyIdleState());
            return;
        }
        
        _timer -= Time.deltaTime;
        float dist = Vector2.Distance(enemy.transform.position, enemy.Target.position);
        
        // 1. Check if we can attack now?
        // Retrying periodically to see if a token opened up
        if (enemy.CanAttack && enemy.IsTargetInAttackRange() && 
            (EnemyManager.Instance == null || EnemyManager.Instance.RequestAttackToken(enemy)))
        {
            enemy.ChangeState(new EnemyAttackState());
            return;
        }
        
        // 2. Movement Logic
        // Calculate vector to target
        Vector2 toTarget = (enemy.Target.position - enemy.transform.position).normalized;
        
        // Calculate tangent (strafe direction)
        Vector2 tangent = new Vector2(-toTarget.y, toTarget.x) * _direction;
        
        // If too far, blend in some forward movement. If too close, blend backward.
        // Maintain roughly 'AttackRange + 1' distance
        float idealDist = enemy.AttackRange + 1.5f;
        Vector2 adjustment = Vector2.zero;
        
        if (dist > idealDist + 0.5f) adjustment = toTarget * 0.5f; // Move closer
        else if (dist < idealDist - 0.5f) adjustment = -toTarget * 0.5f; // Back off
        
        // Combine strafe + adjustment
        Vector2 finalDir = (tangent + adjustment).normalized;
        
        // Separation from other enemies
        Vector2 separation = Vector2.zero;
        if (EnemyManager.Instance != null)
        {
            separation = EnemyManager.Instance.GetSeparationVector(enemy, 1.5f);
        }
        
        Vector2 moveDirection = (finalDir + separation).normalized;
        enemy.SetVelocity(moveDirection * (enemy.MoveSpeed * 0.8f));
        
        // Face target while strafing
        enemy.FaceTarget();
            
        // Timeout -> maybe switch direction or try chasing again
        if (_timer <= 0)
        {
             // Reroll direction
             _timer = Random.Range(1f, 3f);
             _direction *= -1;
             
             // If really far, go back to chase
             if (dist > enemy.DetectionRange * 0.8f)
             {
                 enemy.ChangeState(new EnemyChaseState());
             }
        }
    }
    
    public void Exit(EnemyBase enemy)
    {
    }
}
