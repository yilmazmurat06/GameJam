using UnityEngine;

/// <summary>
/// Sorrow Mask Active Ability: Phase
/// - Pass through iron bars, locked gates, certain guards
/// - Disable collision with tagged objects
/// - Ethereal visual effect
/// </summary>
public class SorrowAbility : MaskAbility
{
    [Header("Phase Settings")]
    [SerializeField] private Color _phaseColor = new Color(0.5f, 0.5f, 1f, 0.3f);
    
    private Color _originalColor;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _playerCollider;
    
    // Layers to phase through
    private int _phasableMask;
    
    protected override void Awake()
    {
        base.Awake();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerCollider = GetComponent<Collider2D>();
        _cooldown = 6f;
        _duration = 5f;
        
        // Setup phasable layer mask
        _phasableMask = LayerMask.GetMask("Phasable", "Enemy");
    }
    
    protected override void OnActivate()
    {
        // Store original color
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
        
        // Apply phase visual
        if (_spriteRenderer != null)
            _spriteRenderer.color = _phaseColor;
        
        // Disable collision with phasable objects
        SetPhaseCollisions(true);
    }
    
    protected override void OnDeactivate()
    {
        // Restore color
        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalColor;
        
        // Re-enable collisions
        SetPhaseCollisions(false);
    }
    
    private void SetPhaseCollisions(bool phasing)
    {
        int playerLayer = gameObject.layer;
        
        // Toggle collision with Phasable layer
        int phasableLayer = LayerMask.NameToLayer("Phasable");
        if (phasableLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, phasableLayer, phasing);
        }
        
        // Optionally phase through enemies (Ghost Guards)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, phasing);
        }
    }
}
