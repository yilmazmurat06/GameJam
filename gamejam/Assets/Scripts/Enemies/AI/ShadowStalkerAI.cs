using UnityEngine;

/// <summary>
/// Shadow Stalker AI for Fear Room (KORKU).
/// Behavior: Lurk in darkness, stalk player, vanish when seen.
/// One-hit kill (player has 1 HP in Fear room).
/// </summary>
public class ShadowStalkerAI : MonoBehaviour
{
    public enum StalkerState { Lurking, Stalking, Vanishing }
    
    [Header("Settings")]
    [SerializeField] private float _detectionRange = 8f;
    [SerializeField] private float _stalkSpeed = 2f;
    [SerializeField] private float _vanishDistance = 2f;
    [SerializeField] private float _respawnDelay = 3f;
    [SerializeField] private float _attackDamage = 999f;
    
    [Header("Visuals")]
    [SerializeField] private Color _lurkColor = new Color(0.1f, 0.1f, 0.15f, 0.3f);
    [SerializeField] private Color _stalkColor = new Color(0.2f, 0.1f, 0.1f, 0.6f);
    
    private StalkerState _state = StalkerState.Lurking;
    private Transform _player;
    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private Vector3 _spawnPosition;
    private float _respawnTimer;
    
    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _spawnPosition = transform.position;
        
        // Find player
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) _player = pc.transform;
        
        // Setup physics
        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }
        
        EnterState(StalkerState.Lurking);
    }
    
    private void Update()
    {
        if (_player == null) return;
        
        switch (_state)
        {
            case StalkerState.Lurking:
                UpdateLurking();
                break;
            case StalkerState.Stalking:
                UpdateStalking();
                break;
            case StalkerState.Vanishing:
                UpdateVanishing();
                break;
        }
    }
    
    private void UpdateLurking()
    {
        float dist = Vector2.Distance(transform.position, _player.position);
        
        // Start stalking if player is in range
        if (dist < _detectionRange)
        {
            EnterState(StalkerState.Stalking);
        }
    }
    
    private void UpdateStalking()
    {
        if (_player == null) return;
        
        float dist = Vector2.Distance(transform.position, _player.position);
        
        // Check if player is "looking" at us (facing our direction)
        PlayerController pc = _player.GetComponent<PlayerController>();
        if (pc != null)
        {
            Vector2 toStalker = (transform.position - _player.position).normalized;
            bool playerFacingUs = (pc.IsFacingRight && toStalker.x > 0.3f) || 
                                  (!pc.IsFacingRight && toStalker.x < -0.3f);
            
            // Vanish if player looks at us
            if (playerFacingUs && dist < _vanishDistance * 2)
            {
                EnterState(StalkerState.Vanishing);
                return;
            }
        }
        
        // Move toward player
        Vector2 dir = (_player.position - transform.position).normalized;
        if (_rb != null)
        {
            _rb.linearVelocity = dir * _stalkSpeed;
        }
        
        // Face player
        if (_sr != null)
        {
            _sr.flipX = dir.x < 0;
        }
    }
    
    private void UpdateVanishing()
    {
        _respawnTimer -= Time.deltaTime;
        
        if (_respawnTimer <= 0)
        {
            // Respawn at original position
            transform.position = _spawnPosition;
            EnterState(StalkerState.Lurking);
        }
    }
    
    private void EnterState(StalkerState newState)
    {
        _state = newState;
        
        switch (newState)
        {
            case StalkerState.Lurking:
                if (_sr != null) _sr.color = _lurkColor;
                if (_rb != null) _rb.linearVelocity = Vector2.zero;
                break;
                
            case StalkerState.Stalking:
                if (_sr != null) _sr.color = _stalkColor;
                break;
                
            case StalkerState.Vanishing:
                if (_sr != null) _sr.color = new Color(0, 0, 0, 0);
                if (_rb != null) _rb.linearVelocity = Vector2.zero;
                _respawnTimer = _respawnDelay;
                break;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        // Attack player on touch
        if (col.gameObject.CompareTag("Player") || col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Health playerHealth = col.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                DamageInfo dmg = new DamageInfo(_attackDamage, gameObject);
                playerHealth.TakeDamage(dmg);
                Debug.Log("[ShadowStalker] Killed player!");
            }
            
            EnterState(StalkerState.Vanishing);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _vanishDistance);
    }
}
