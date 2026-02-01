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
        
        // Always move toward player (Solid Stalker)
        Vector2 dir = (_player.position - transform.position).normalized;
        if (_rb != null)
        {
            _rb.velocity = dir * _stalkSpeed;
        }
        
        // Face player
        if (_sr != null)
        {
            _sr.flipX = dir.x < 0;
        }
    }
    
    private void EnterState(StalkerState newState)
    {
        _state = newState;
        
        switch (newState)
        {
            case StalkerState.Lurking:
                if (_rb != null) _rb.velocity = Vector2.zero;
                break;
                
            case StalkerState.Stalking:
                // Full speed
                break;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        // Attack player on touch (Physical contact)
        if (col.gameObject.CompareTag("Player") || col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Health playerHealth = col.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                DamageInfo dmg = new DamageInfo(_attackDamage, gameObject);
                playerHealth.TakeDamage(dmg);
                Debug.Log("[ShadowStalker] Contact! Dealing " + _attackDamage + " damage.");
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
    }
}
