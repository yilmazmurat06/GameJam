using UnityEngine;

/// <summary>
/// Enemy patrol state - wanders around patrol origin.
/// SoulKnight-style patrol behavior.
/// </summary>
public class EnemyPatrolState : IEnemyState
{
    private Vector2 _patrolOrigin;
    private float _patrolRadius;
    private float _waitTime;
    
    private Vector2 _currentDestination;
    private float _waitTimer;
    private bool _isWaiting;
    private bool _hasDestination;
    
    public EnemyPatrolState(Vector2 origin, float radius, float waitTime)
    {
        _patrolOrigin = origin;
        _patrolRadius = radius;
        _waitTime = waitTime;
    }
    
    public void Enter(EnemyBase enemy)
    {
        _isWaiting = true;
        _waitTimer = _waitTime * 0.5f; // Start with half wait time
        _hasDestination = false;
        enemy.SetVelocity(Vector2.zero);
    }
    
    public void Execute(EnemyBase enemy)
    {
        // Check for target - switch to chase
        if (enemy.HasTarget)
        {
            enemy.ChangeState(new EnemyChaseState());
            return;
        }
        
        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0)
            {
                _isWaiting = false;
                PickNewDestination(enemy);
            }
            return;
        }
        
        // Move toward destination
        if (_hasDestination)
        {
            float distToDest = Vector2.Distance(enemy.transform.position, _currentDestination);
            
            if (distToDest < 0.3f)
            {
                // Reached destination, wait
                _isWaiting = true;
                _waitTimer = _waitTime + Random.Range(-0.5f, 0.5f);
                enemy.SetVelocity(Vector2.zero);
            }
            else
            {
                // Move toward destination at reduced speed
                enemy.MoveToward(_currentDestination);
            }
        }
    }
    
    public void Exit(EnemyBase enemy)
    {
        enemy.SetVelocity(Vector2.zero);
    }
    
    private void PickNewDestination(EnemyBase enemy)
    {
        // Pick random point within patrol radius
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(_patrolRadius * 0.3f, _patrolRadius);
        _currentDestination = _patrolOrigin + randomDir * randomDist;
        _hasDestination = true;
    }
}
