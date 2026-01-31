using UnityEngine;

/// <summary>
/// Enemy idle state - patrols/roams randomly while looking for player.
/// </summary>
public class EnemyIdleState : IEnemyState
{
    private float _idleTimer;
    private float _moveDuration;
    private Vector2 _wanderTarget;
    private bool _isMoving;
    
    // Roaming settings
    private const float MinIdleTime = 1f;
    private const float MaxIdleTime = 3f;
    private const float WanderRadius = 3f;
    private const float ReachedDistance = 0.1f;
    
    public void Enter(EnemyBase enemy)
    {
        // Start by waiting a bit
        _idleTimer = Random.Range(MinIdleTime, MaxIdleTime);
        _isMoving = false;
        enemy.SetVelocity(Vector2.zero);
    }
    
    public void Execute(EnemyBase enemy)
    {
        // 1. Check for Player (Priority)
        if (enemy.HasTarget)
        {
            enemy.ChangeState(new EnemyChaseState());
            return;
        }
        
        // 2. Handle Roaming Logic
        if (_isMoving)
        {
            // Move toward target
            float distance = Vector2.Distance(enemy.transform.position, _wanderTarget);
            
            if (distance <= ReachedDistance)
            {
                // Reached destination, start waiting
                _isMoving = false;
                _idleTimer = Random.Range(MinIdleTime, MaxIdleTime);
                enemy.SetVelocity(Vector2.zero);
            }
            else
            {
                // Keep moving
                enemy.MoveToward(_wanderTarget);
            }
        }
        else
        {
            // Waiting
            _idleTimer -= Time.deltaTime;
            
            if (_idleTimer <= 0)
            {
                // Pick new target
                _wanderTarget = (Vector2)enemy.transform.position + Random.insideUnitCircle * WanderRadius;
                _isMoving = true;
            }
        }
    }
    
    public void Exit(EnemyBase enemy)
    {
        // Cleanup
        enemy.SetVelocity(Vector2.zero);
    }
}
