using UnityEngine;

/// <summary>
/// Chain Demon AI for Guilt Room (SUÇLULUK).
/// Behavior: Waits, grabs player, drags them backward.
/// Player uses Anchor ability to resist.
/// </summary>
public class ChainDemonAI : MonoBehaviour
{
    public enum DemonState { Waiting, Grabbing, Dragging, Releasing }
    
    [Header("Settings")]
    [SerializeField] private float _grabRange = 5f;
    [SerializeField] private float _dragForce = 8f;
    [SerializeField] private float _grabDuration = 0.5f;
    [SerializeField] private float _dragDuration = 2f;
    [SerializeField] private float _releaseCooldown = 3f;
    [SerializeField] private float _massThreshold = 500f; // Player needs Anchor to resist
    
    [Header("Visuals")]
    [SerializeField] private Color _waitColor = new Color(0.3f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color _grabColor = new Color(0.6f, 0.1f, 0.1f, 1f);
    
    private DemonState _state = DemonState.Waiting;
    private Transform _player;
    private Rigidbody2D _playerRb;
    private SpriteRenderer _sr;
    private float _stateTimer;
    private LineRenderer _chainLine;
    
    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        
        // Find player
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            _player = pc.transform;
            _playerRb = pc.Rigidbody;
        }
        
        // Setup chain visual
        _chainLine = gameObject.AddComponent<LineRenderer>();
        _chainLine.startWidth = 0.1f;
        _chainLine.endWidth = 0.05f;
        _chainLine.material = new Material(Shader.Find("Sprites/Default"));
        _chainLine.startColor = Color.gray;
        _chainLine.endColor = Color.black;
        _chainLine.positionCount = 2;
        _chainLine.enabled = false;
        
        if (_sr != null) _sr.color = _waitColor;
    }
    
    private void Update()
    {
        if (_player == null) return;
        
        switch (_state)
        {
            case DemonState.Waiting:
                UpdateWaiting();
                break;
            case DemonState.Grabbing:
                UpdateGrabbing();
                break;
            case DemonState.Dragging:
                UpdateDragging();
                break;
            case DemonState.Releasing:
                UpdateReleasing();
                break;
        }
    }
    
    private void UpdateWaiting()
    {
        float dist = Vector2.Distance(transform.position, _player.position);
        
        if (dist < _grabRange)
        {
            EnterState(DemonState.Grabbing);
        }
    }
    
    private void UpdateGrabbing()
    {
        _stateTimer -= Time.deltaTime;
        
        // Animate chain extending
        UpdateChainVisual();
        
        if (_stateTimer <= 0)
        {
            EnterState(DemonState.Dragging);
        }
    }
    
    private void UpdateDragging()
    {
        _stateTimer -= Time.deltaTime;
        
        UpdateChainVisual();
        
        // Check if player can resist (high mass from Anchor)
        if (_playerRb != null && _playerRb.mass >= _massThreshold)
        {
            Debug.Log("[ChainDemon] Player resists with Anchor!");
            EnterState(DemonState.Releasing);
            return;
        }
        
        // Drag player toward demon
        if (_playerRb != null)
        {
            Vector2 dragDir = (transform.position - _player.position).normalized;
            _playerRb.AddForce(dragDir * _dragForce, ForceMode2D.Force);
        }
        
        if (_stateTimer <= 0)
        {
            EnterState(DemonState.Releasing);
        }
    }
    
    private void UpdateReleasing()
    {
        _stateTimer -= Time.deltaTime;
        
        if (_stateTimer <= 0)
        {
            EnterState(DemonState.Waiting);
        }
    }
    
    private void EnterState(DemonState newState)
    {
        _state = newState;
        
        switch (newState)
        {
            case DemonState.Waiting:
                if (_sr != null) _sr.color = _waitColor;
                if (_chainLine != null) _chainLine.enabled = false;
                break;
                
            case DemonState.Grabbing:
                _stateTimer = _grabDuration;
                if (_sr != null) _sr.color = _grabColor;
                if (_chainLine != null) _chainLine.enabled = true;
                Debug.Log("[ChainDemon] Grabbing player!");
                break;
                
            case DemonState.Dragging:
                _stateTimer = _dragDuration;
                Debug.Log("[ChainDemon] Dragging player!");
                break;
                
            case DemonState.Releasing:
                _stateTimer = _releaseCooldown;
                if (_chainLine != null) _chainLine.enabled = false;
                if (_sr != null) _sr.color = _waitColor;
                break;
        }
    }
    
    private void UpdateChainVisual()
    {
        if (_chainLine == null || _player == null) return;
        
        _chainLine.SetPosition(0, transform.position);
        _chainLine.SetPosition(1, _player.position);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _grabRange);
    }
}
