using UnityEngine;

/// <summary>
/// Ghost Guard AI for Sorrow Room (HÜZÜN).
/// Behavior: Slow patrol, blocks paths, ignores phased player.
/// Non-aggressive - just obstructs passage.
/// </summary>
public class GhostGuardAI : MonoBehaviour
{
    public enum GuardState { Patrol, Blocking, Weeping }
    
    [Header("Settings")]
    [SerializeField] private float _patrolSpeed = 1f;
    [SerializeField] private float _blockDuration = 3f;
    [SerializeField] private float _weepDuration = 2f;
    [SerializeField] private float _weepChance = 0.2f;
    
    [Header("Patrol Path")]
    [SerializeField] private Transform[] _patrolPoints;
    private int _currentPointIndex;
    
    [Header("Visuals")]
    [SerializeField] private Color _normalColor = new Color(0.6f, 0.6f, 0.8f, 0.7f);
    [SerializeField] private Color _weepingColor = new Color(0.4f, 0.4f, 0.7f, 0.5f);
    
    private GuardState _state = GuardState.Patrol;
    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private float _stateTimer;
    
    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        
        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }
        
        if (_sr != null) _sr.color = _normalColor;
    }
    
    private void Update()
    {
        switch (_state)
        {
            case GuardState.Patrol:
                UpdatePatrol();
                break;
            case GuardState.Blocking:
                UpdateBlocking();
                break;
            case GuardState.Weeping:
                UpdateWeeping();
                break;
        }
    }
    
    private void UpdatePatrol()
    {
        if (_patrolPoints == null || _patrolPoints.Length == 0)
        {
            // No patrol points - just stand
            if (_rb != null) _rb.velocity = Vector2.zero;
            return;
        }
        
        // Move toward current patrol point
        Transform target = _patrolPoints[_currentPointIndex];
        if (target == null) return;
        
        Vector2 dir = (target.position - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, target.position);
        
        if (dist < 0.3f)
        {
            // Reached point - maybe weep, maybe continue
            if (Random.value < _weepChance)
            {
                EnterState(GuardState.Weeping);
            }
            else
            {
                _currentPointIndex = (_currentPointIndex + 1) % _patrolPoints.Length;
            }
            return;
        }
        
        if (_rb != null)
        {
            _rb.velocity = dir * _patrolSpeed;
        }
        
        // Face direction
        if (_sr != null && dir.x != 0)
        {
            _sr.flipX = dir.x < 0;
        }
    }
    
    private void UpdateBlocking()
    {
        if (_rb != null) _rb.velocity = Vector2.zero;
        
        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0)
        {
            EnterState(GuardState.Patrol);
        }
    }
    
    private void UpdateWeeping()
    {
        if (_rb != null) _rb.velocity = Vector2.zero;
        
        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0)
        {
            EnterState(GuardState.Patrol);
        }
    }
    
    private void EnterState(GuardState newState)
    {
        _state = newState;
        
        switch (newState)
        {
            case GuardState.Patrol:
                if (_sr != null) _sr.color = _normalColor;
                break;
            case GuardState.Blocking:
                _stateTimer = _blockDuration;
                break;
            case GuardState.Weeping:
                _stateTimer = _weepDuration;
                if (_sr != null) _sr.color = _weepingColor;
                Debug.Log("[GhostGuard] *weeping*");
                break;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        // Block player path (non-damaging)
        if (col.gameObject.CompareTag("Player") || col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            EnterState(GuardState.Blocking);
        }
    }
}
