using UnityEngine;

/// <summary>
/// Heavy object for Guilt Room.
/// Can only be pushed when player uses Anchor ability (high mass).
/// </summary>
public class HeavyObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _pushThreshold = 500f; // Min player mass to push
    [SerializeField] private float _friction = 5f;
    
    [Header("Visual")]
    [SerializeField] private Color _immovableColor = new Color(0.4f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color _movableColor = new Color(0.6f, 0.5f, 0.5f, 1f);
    
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private bool _canBePushed;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        
        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.mass = 100f;
            _rb.linearDamping = _friction;
            _rb.freezeRotation = true;
        }
        
        UpdateVisual();
    }
    
    private void Update()
    {
        // Check if player can push (has high mass from Anchor)
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.Rigidbody != null)
        {
            _canBePushed = player.Rigidbody.mass >= _pushThreshold;
        }
        else
        {
            _canBePushed = false;
        }
        
        UpdateVisual();
        
        // Freeze if not pushable
        if (_rb != null)
        {
            _rb.constraints = _canBePushed 
                ? RigidbodyConstraints2D.FreezeRotation 
                : RigidbodyConstraints2D.FreezeAll;
        }
    }
    
    private void UpdateVisual()
    {
        if (_sr != null)
        {
            _sr.color = _canBePushed ? _movableColor : _immovableColor;
        }
    }
    
    private void OnCollisionStay2D(Collision2D col)
    {
        if (!_canBePushed) return;
        
        // Only allow player to push
        if (col.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        
        // Physics will handle the push naturally when constraints are released
    }
}
