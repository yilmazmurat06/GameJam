using UnityEngine;

/// <summary>
/// Rage Golem AI for Hate Room (NEFRET).
/// Behavior: Aggressive charges, destroys environment on impact.
/// Fast, predictable, can be dodged.
/// </summary>
public class RageGolemAI : MonoBehaviour
{
    public enum GolemState { Patrol, Charging, Stunned }
    
    [Header("Settings")]
    [SerializeField] private float _patrolSpeed = 2f;
    [SerializeField] private float _chargeSpeed = 12f;
    [SerializeField] private float _detectionRange = 6f;
    [SerializeField] private float _chargeWindup = 0.5f;
    [SerializeField] private float _stunDuration = 1.5f;
    [SerializeField] private float _attackDamage = 30f;
    
    [Header("Patrol")]
    [SerializeField] private Vector2 _patrolDirection = Vector2.right;
    [SerializeField] private float _patrolDistance = 3f;
    
    private GolemState _state = GolemState.Patrol;
    private Transform _player;
    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private Vector3 _startPosition;
    private Vector2 _chargeDirection;
    private float _stateTimer;
    private bool _chargeReady;
    
    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _startPosition = transform.position;
        
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) _player = pc.transform;
        
        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }
    }
    
    private void Update()
    {
        if (_player == null) return;
        
        switch (_state)
        {
            case GolemState.Patrol:
                UpdatePatrol();
                break;
            case GolemState.Charging:
                UpdateCharging();
                break;
            case GolemState.Stunned:
                UpdateStunned();
                break;
        }
    }
    
    private void UpdatePatrol()
    {
        float dist = Vector2.Distance(transform.position, _player.position);
        
        // Detect player
        if (dist < _detectionRange)
        {
            StartCharge();
            return;
        }
        
        // Simple back-and-forth patrol
        float traveled = Vector3.Distance(transform.position, _startPosition);
        if (traveled > _patrolDistance)
        {
            _patrolDirection = -_patrolDirection;
            _startPosition = transform.position;
        }
        
        if (_rb != null)
        {
            _rb.velocity = _patrolDirection * _patrolSpeed;
        }
        
        // Face direction
        if (_sr != null && _patrolDirection.x != 0)
        {
            _sr.flipX = _patrolDirection.x < 0;
        }
    }
    
    private void StartCharge()
    {
        _state = GolemState.Charging;
        _chargeReady = false;
        _stateTimer = _chargeWindup;
        
        // Lock charge direction toward player
        _chargeDirection = (_player.position - transform.position).normalized;
        
        // Stop during windup
        if (_rb != null) _rb.velocity = Vector2.zero;
        
        // Windup visual (red tint)
        if (_sr != null) _sr.color = new Color(1f, 0.5f, 0.5f, 1f);
        
        Debug.Log("[RageGolem] CHARGING!");
    }
    
    private void UpdateCharging()
    {
        if (!_chargeReady)
        {
            // Windup phase
            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0)
            {
                _chargeReady = true;
                if (_sr != null) _sr.color = Color.red;
            }
            return;
        }
        
        // Charging phase
        if (_rb != null)
        {
            _rb.velocity = _chargeDirection * _chargeSpeed;
        }
    }
    
    private void UpdateStunned()
    {
        _stateTimer -= Time.deltaTime;
        
        if (_stateTimer <= 0)
        {
            _state = GolemState.Patrol;
            if (_sr != null) _sr.color = Color.white;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_state == GolemState.Stunned) return;
        
        // Hit player
        if (col.gameObject.CompareTag("Player") || col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Health playerHealth = col.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                DamageInfo dmg = new DamageInfo(_attackDamage, gameObject).WithKnockback(_chargeDirection * 15f);
                playerHealth.TakeDamage(dmg);
            }
        }
        
        // Check for destructibles
        if (col.gameObject.CompareTag("CrackedWall") || col.gameObject.CompareTag("BrokenFurniture"))
        {
            Destroy(col.gameObject);
            Debug.Log($"[RageGolem] Destroyed {col.gameObject.name}!");
        }
        
        // Hit wall = stunned
        if (_state == GolemState.Charging)
        {
            _state = GolemState.Stunned;
            _stateTimer = _stunDuration;
            if (_rb != null) _rb.velocity = Vector2.zero;
            if (_sr != null) _sr.color = Color.yellow;
            Debug.Log("[RageGolem] STUNNED!");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
    }
}
