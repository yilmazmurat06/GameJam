using UnityEngine;

/// <summary>
/// Guilt Mask Active Ability: Anchor
/// - Temporarily set mass to 1000
/// - Push heavy regret blocks
/// - Resist wind tunnels
/// - Hold pressure plates
/// </summary>
public class GuiltAbility : MaskAbility
{
    [Header("Anchor Settings")]
    [SerializeField] private float _anchorMass = 1000f;
    [SerializeField] private Color _anchorColor = new Color(0.4f, 0.3f, 0.3f, 1f);
    
    private float _originalMass;
    private Color _originalColor;
    private SpriteRenderer _spriteRenderer;
    
    protected override void Awake()
    {
        base.Awake();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _cooldown = 4f;
        _duration = 3f;
    }
    
    protected override void OnActivate()
    {
        // Store originals
        _originalMass = _player.Rigidbody.mass;
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
        
        // Apply anchor mass
        _player.Rigidbody.mass = _anchorMass;
        
        // Visual: darker, heavier look
        if (_spriteRenderer != null)
            _spriteRenderer.color = _anchorColor;
        
        Debug.Log($"[GuiltAbility] Anchor activated. Mass: {_anchorMass}");
    }
    
    protected override void OnDeactivate()
    {
        // Restore original mass (which was set by the Guilt mask passive effect)
        _player.Rigidbody.mass = _originalMass;
        
        // Restore color
        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalColor;
        
        Debug.Log("[GuiltAbility] Anchor deactivated.");
    }
}
