using UnityEngine;

/// <summary>
/// Hate Mask Active Ability: Shield Bash
/// - Short, aggressive dash forward
/// - Destroys objects with tag: CrackedWall, BrokenFurniture
/// - Deals damage to enemies on contact
/// </summary>
public class HateAbility : MaskAbility
{
    [Header("Shield Bash Settings")]
    [SerializeField] private float _dashDistance = 5f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _bashDamage = 50f;
    [SerializeField] private LayerMask _destructibleLayers;
    
    private Vector2 _dashDirection;
    private float _dashTimer;
    private bool _isDashing;
    
    protected override void Awake()
    {
        base.Awake();
        _cooldown = 3f;
        _duration = 0.3f; // Short burst
    }
    
    protected override void OnActivate()
    {
        // Get dash direction from input or facing
        Vector2 input = _player.InputHandler.MoveInput;
        if (input.magnitude < 0.1f)
        {
            // Default to facing direction
            _dashDirection = _player.IsFacingRight ? Vector2.right : Vector2.left;
        }
        else
        {
            _dashDirection = input.normalized;
        }
        
        _isDashing = true;
        _dashTimer = _dashDuration;
        
        // Disable normal movement during dash
        _player.Rigidbody.linearVelocity = Vector2.zero;
    }
    
    protected override void OnDeactivate()
    {
        _isDashing = false;
        _player.Rigidbody.linearVelocity = Vector2.zero;
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            
            // Move in dash direction
            float speed = _dashDistance / _dashDuration;
            _player.Rigidbody.linearVelocity = _dashDirection * speed;
            
            // Check for destructibles
            CheckDestructibles();
            
            if (_dashTimer <= 0)
            {
                Deactivate();
            }
        }
    }
    
    private void CheckDestructibles()
    {
        // Raycast or overlap check for destructible objects
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            // Check for destructible tags
            if (hit.CompareTag("CrackedWall") || hit.CompareTag("BrokenFurniture"))
            {
                Destroy(hit.gameObject);
                Debug.Log($"[HateAbility] Destroyed {hit.name}!");
            }
            
            // Damage enemies
            Health enemyHealth = hit.GetComponent<Health>();
            if (enemyHealth != null && hit.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                DamageInfo dmg = new DamageInfo(_bashDamage, gameObject).WithKnockback(_dashDirection * 10f);
                enemyHealth.TakeDamage(dmg);
            }
        }
    }
}
