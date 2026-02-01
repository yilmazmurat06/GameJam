using UnityEngine;

/// <summary>
/// SoulKnight-style enemy AI brain using behavior trees / priority system.
/// Handles decision making separately from state execution.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float _detectionRange = 8f;
    [SerializeField] private float _loseTargetRange = 12f;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _preferredDistance = 2f; // Ranged enemies stay back
    [SerializeField] private bool _isRanged = false;
    
    [Header("Behavior Weights")]
    [SerializeField] private float _aggressiveness = 0.7f; // 0-1, higher = more likely to attack
    [SerializeField] private float _fleeHealthThreshold = 0.2f; // Flee when below this health %
    
    [Header("Patrol Settings")]
    [SerializeField] private bool _canPatrol = true;
    [SerializeField] private float _patrolRadius = 5f;
    [SerializeField] private float _patrolWaitTime = 2f;
    
    // References
    private EnemyBase _enemy;
    private Transform _target;
    private Vector2 _patrolOrigin;
    private Vector2 _patrolDestination;
    private float _patrolTimer;
    private bool _isPatrolling;
    
    // State
    public AIState CurrentState { get; private set; } = AIState.Idle;
    public bool HasTarget => _target != null;
    public Transform Target => _target;
    public float DistanceToTarget => _target != null ? Vector2.Distance(transform.position, _target.position) : float.MaxValue;
    public bool IsInAttackRange => DistanceToTarget <= _attackRange;
    public bool IsInDetectionRange => DistanceToTarget <= _detectionRange;
    
    public enum AIState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Strafe,
        Flee,
        Dead
    }
    
    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _patrolOrigin = transform.position;
    }
    
    private void Start()
    {
        if (_enemy.Health != null)
        {
            _enemy.Health.OnDeath += () => CurrentState = AIState.Dead;
        }
    }
    
    private void Update()
    {
        if (CurrentState == AIState.Dead) return;
        
        // Always try to find target
        UpdateTarget();
        
        // Decide next action
        DecideAction();
    }
    
    /// <summary>
    /// Find and track the player.
    /// </summary>
    private void UpdateTarget()
    {
        // If we have a target, check if still valid
        if (_target != null)
        {
            // Check if target is dead or too far
            Health targetHealth = _target.GetComponent<Health>();
            if (targetHealth != null && !targetHealth.IsAlive)
            {
                _target = null;
                return;
            }
            
            // Lose target if too far
            if (DistanceToTarget > _loseTargetRange)
            {
                _target = null;
                return;
            }
        }
        
        // Search for new target if none
        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist <= _detectionRange)
                {
                    _target = player.transform;
                    _enemy.SetTarget(_target);
                }
            }
        }
    }
    
    /// <summary>
    /// Main decision-making logic (priority-based).
    /// </summary>
    private void DecideAction()
    {
        // Priority 1: Check if should flee (low health)
        if (ShouldFlee())
        {
            SetState(AIState.Flee);
            return;
        }
        
        // Priority 2: Attack if in range and can attack
        if (HasTarget && IsInAttackRange && _enemy.CanAttack)
        {
            // Request attack token from manager
            if (EnemyManager.Instance == null || EnemyManager.Instance.RequestAttackToken(_enemy))
            {
                SetState(AIState.Attack);
                return;
            }
            else
            {
                // Can't attack, strafe instead
                SetState(AIState.Strafe);
                return;
            }
        }
        
        // Priority 3: Chase target
        if (HasTarget)
        {
            // Ranged enemies try to maintain distance
            if (_isRanged && DistanceToTarget < _preferredDistance * 0.8f)
            {
                SetState(AIState.Strafe); // Back up
            }
            else
            {
                SetState(AIState.Chase);
            }
            return;
        }
        
        // Priority 4: Patrol or idle
        if (_canPatrol)
        {
            SetState(AIState.Patrol);
        }
        else
        {
            SetState(AIState.Idle);
        }
    }
    
    /// <summary>
    /// Check if enemy should flee.
    /// </summary>
    private bool ShouldFlee()
    {
        if (_enemy.Health == null) return false;
        return _enemy.Health.HealthPercent < _fleeHealthThreshold;
    }
    
    /// <summary>
    /// Set the AI state and trigger state change on enemy.
    /// </summary>
    private void SetState(AIState newState)
    {
        if (CurrentState == newState) return;
        
        AIState previousState = CurrentState;
        CurrentState = newState;
        
        // Trigger appropriate enemy state
        switch (newState)
        {
            case AIState.Idle:
                _enemy.ChangeState(new EnemyIdleState());
                break;
            case AIState.Patrol:
                _enemy.ChangeState(new EnemyPatrolState(_patrolOrigin, _patrolRadius, _patrolWaitTime));
                break;
            case AIState.Chase:
                _enemy.ChangeState(new EnemyChaseState());
                break;
            case AIState.Attack:
                _enemy.ChangeState(new EnemyAttackState());
                break;
            case AIState.Strafe:
                _enemy.ChangeState(new EnemyStrafeState());
                break;
            case AIState.Flee:
                _enemy.ChangeState(new EnemyFleeState());
                break;
            case AIState.Dead:
                _enemy.ChangeState(new EnemyDeathState());
                break;
        }
        
        Debug.Log($"[EnemyAI] {_enemy.EnemyName}: {previousState} -> {newState}");
    }
    
    /// <summary>
    /// Get direction away from target for fleeing/strafing.
    /// </summary>
    public Vector2 GetFleeDirection()
    {
        if (!HasTarget) return Vector2.zero;
        return ((Vector2)transform.position - (Vector2)_target.position).normalized;
    }
    
    /// <summary>
    /// Get strafe direction (perpendicular to target).
    /// </summary>
    public Vector2 GetStrafeDirection()
    {
        if (!HasTarget) return Vector2.zero;
        Vector2 toTarget = ((Vector2)_target.position - (Vector2)transform.position).normalized;
        // Randomly choose left or right strafe
        return Random.value > 0.5f ? new Vector2(-toTarget.y, toTarget.x) : new Vector2(toTarget.y, -toTarget.x);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
        
        // Preferred distance (ranged)
        if (_isRanged)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _preferredDistance);
        }
        
        // Patrol area
        if (_canPatrol)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_patrolOrigin != Vector2.zero ? (Vector3)_patrolOrigin : transform.position, _patrolRadius);
        }
    }
}
