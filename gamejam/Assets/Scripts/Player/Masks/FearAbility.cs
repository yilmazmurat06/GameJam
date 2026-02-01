using UnityEngine;

/// <summary>
/// Fear Mask Active Ability: Shadow Form
/// - Become untargetable by enemies
/// - Movement speed significantly reduced
/// - Cannot interact with enemies (no damage dealt or received)
/// </summary>
public class FearAbility : MaskAbility
{
    [Header("Shadow Form Settings")]
    [SerializeField] private float _speedMultiplier = 0.3f;
    [SerializeField] private Color _shadowColor = new Color(0.2f, 0.2f, 0.3f, 0.4f);
    
    private float _originalSpeed;
    private Color _originalColor;
    private SpriteRenderer _spriteRenderer;
    private int _originalLayer;
    
    protected override void Awake()
    {
        base.Awake();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _cooldown = 8f;
        _duration = 4f;
    }
    
    protected override void OnActivate()
    {
        // Store originals
        _originalSpeed = _player.MoveSpeed;
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
        _originalLayer = gameObject.layer;
        
        // Apply shadow form
        // Set to layer that enemies ignore
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        // Visual: dark, transparent
        if (_spriteRenderer != null)
            _spriteRenderer.color = _shadowColor;
        
        // Slow movement (handled via PlayerController speed modifier)
        // We'll slow via velocity reduction in Update
    }
    
    protected override void OnDeactivate()
    {
        // Restore layer
        gameObject.layer = _originalLayer;
        
        // Restore color
        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalColor;
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Speed reduction is now handled by clamping max velocity magnitude
        // instead of multiplying each frame (which would compound to zero)
        if (_isActive && _player != null && _player.Rigidbody != null)
        {
            Vector2 velocity = _player.Rigidbody.linearVelocity;
            float maxSpeed = _player.MoveSpeed * _speedMultiplier;
            if (velocity.magnitude > maxSpeed)
            {
                _player.Rigidbody.linearVelocity = velocity.normalized * maxSpeed;
            }
        }
    }
}
