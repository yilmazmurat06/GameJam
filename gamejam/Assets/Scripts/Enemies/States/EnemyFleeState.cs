using UnityEngine;

/// <summary>
/// Enemy flee state - runs away from target when low health.
/// SoulKnight-style retreat behavior.
/// </summary>
public class EnemyFleeState : IEnemyState
{
    private float _fleeTimer;
    private const float FLEE_DURATION = 3f;
    
    public void Enter(EnemyBase enemy)
    {
        _fleeTimer = FLEE_DURATION;
        Debug.Log($"[{enemy.EnemyName}] Fleeing!");
    }
    
    public void Execute(EnemyBase enemy)
    {
        _fleeTimer -= Time.deltaTime;
        
        // Stop fleeing after duration
        if (_fleeTimer <= 0)
        {
            enemy.ChangeState(new EnemyIdleState());
            return;
        }
        
        // Get flee direction (away from target)
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        Vector2 fleeDir = ai != null ? ai.GetFleeDirection() : -enemy.transform.right;
        
        // Move away at increased speed
        Vector2 fleeVelocity = fleeDir * enemy.MoveSpeed * 1.5f;
        enemy.SetVelocity(fleeVelocity);
    }
    
    public void Exit(EnemyBase enemy)
    {
        enemy.SetVelocity(Vector2.zero);
    }
}
