using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles 4-directional sprite animation based on movement direction.
/// Auto-detects row count based on total sprites / columns.
/// Row order depends on sprite sheet - defaults assume standard RPG format.
/// </summary>
public class EnemyAnimator : MonoBehaviour
{
    [Header("Sprite Sheet Settings")]
    [SerializeField] private List<Sprite> _allSprites = new List<Sprite>();
    [SerializeField] private int _columns = 8;
    [SerializeField] private float _animationSpeed = 0.12f;
    
    // Row indices - ADJUSTABLE based on sprite sheet layout
    // Standard RPG-Maker style: Down=0, Left=1, Right=2, Up=3
    // If your sheet is different, adjust these in Inspector or here
    [Header("Direction Row Indices")]
    public int RowDown = 0;
    public int RowLeft = 1; 
    public int RowRight = 2;
    public int RowUp = 3;
    
    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private float _timer;
    private int _currentFrame;
    private int _currentRow;
    private Vector2 _lastDirection = Vector2.down; // Default facing
    private Vector2 _externalDirection; // Direction set externally (for when velocity doesn't update correctly)
    private bool _useExternalDirection;
    
    /// <summary>
    /// Returns true if this animator has enough sprites for 4-directional animation.
    /// Requires at least 4 rows of sprites (4 * columns).
    /// </summary>
    public bool HasDirectionalSprites => _allSprites != null && _allSprites.Count >= _columns * 4 && _columns > 1;
    
    /// <summary>
    /// Gets the current movement direction being used for animation.
    /// </summary>
    public Vector2 CurrentDirection => _lastDirection;

    public void Initialize(List<Sprite> sprites, int columns = 8)
    {
        _allSprites = sprites;
        _columns = columns;
        
        // Auto-detect row count
        int rowCount = _allSprites.Count / _columns;
        Debug.Log($"[EnemyAnimator] Initialized with {_allSprites.Count} sprites, {_columns} cols, ~{rowCount} rows");
    }

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
    }
    
    /// <summary>
    /// Manually set the facing direction. Use this when movement doesn't rely on Rigidbody velocity.
    /// Also handles simple flip for non-directional sprites.
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            _externalDirection = direction.normalized;
            _useExternalDirection = true;
            _lastDirection = _externalDirection;
            
            // For simple flip mode, update immediately
            if (_sr != null && !HasDirectionalSprites && Mathf.Abs(direction.x) > 0.1f)
            {
                _sr.flipX = direction.x < 0;
            }
        }
    }
    
    /// <summary>
    /// Clear external direction and revert to velocity-based animation.
    /// </summary>
    public void ClearExternalDirection()
    {
        _useExternalDirection = false;
    }

    private void Update()
    {
        if (_allSprites == null || _allSprites.Count < _columns)
        {
            // Not enough sprites, fall back to simple flip
            HandleSimpleFlip();
            return;
        }
        
        // Get direction - prioritize external direction if set, otherwise use velocity
        Vector2 moveDir = Vector2.zero;
        bool isMoving = false;
        
        if (_useExternalDirection && _externalDirection.sqrMagnitude > 0.01f)
        {
            moveDir = _externalDirection;
            isMoving = true;
            // Clear after use (requires continuous updates)
            _useExternalDirection = false;
        }
        else if (_rb != null)
        {
            Vector2 vel = _rb.linearVelocity;
            isMoving = vel.magnitude > 0.1f;
            if (isMoving)
            {
                moveDir = vel.normalized;
            }
        }
        
        if (isMoving)
        {
            _lastDirection = moveDir;
            
            // Determine dominant axis for direction
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
            {
                // Horizontal movement dominates
                _currentRow = moveDir.x > 0 ? RowRight : RowLeft;
            }
            else
            {
                // Vertical movement dominates  
                _currentRow = moveDir.y > 0 ? RowUp : RowDown;
            }
            
            // Animate
            _timer += Time.deltaTime;
            if (_timer >= _animationSpeed)
            {
                _timer = 0;
                _currentFrame = (_currentFrame + 1) % _columns;
            }
        }
        else
        {
            // Idle - use first frame of current direction
            _currentFrame = 0;
        }
        
        // Apply sprite - ensure row is valid
        int rowCount = _allSprites.Count / _columns;
        _currentRow = Mathf.Clamp(_currentRow, 0, rowCount - 1);
        
        int index = (_currentRow * _columns) + _currentFrame;
        if (index >= 0 && index < _allSprites.Count && _sr != null)
        {
            _sr.sprite = _allSprites[index];
        }
    }
    
    private void HandleSimpleFlip()
    {
        if (_sr == null) return;
        
        Vector2 dir = Vector2.zero;
        
        // Check external direction first
        if (_useExternalDirection && _externalDirection.sqrMagnitude > 0.01f)
        {
            dir = _externalDirection;
            _useExternalDirection = false; // Clear after use
        }
        else if (_rb != null)
        {
            dir = _rb.linearVelocity;
        }
        
        // Flip sprite based on horizontal movement
        if (Mathf.Abs(dir.x) > 0.1f)
        {
            _sr.flipX = dir.x < 0;
        }
        
        // Update last direction for facing
        if (dir.sqrMagnitude > 0.01f)
        {
            _lastDirection = dir.normalized;
        }
    }
    
    /// <summary>
    /// Force set the current row (for idle facing direction).
    /// Also handles simple flip for non-directional sprites.
    /// </summary>
    public void SetFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;
        
        _lastDirection = direction.normalized;
        
        // For simple flip mode (no directional sprites)
        if (_sr != null && !HasDirectionalSprites)
        {
            if (Mathf.Abs(direction.x) > 0.1f)
            {
                _sr.flipX = direction.x < 0;
            }
            return;
        }
        
        // For directional sprites
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            _currentRow = direction.x > 0 ? RowRight : RowLeft;
        }
        else
        {
            _currentRow = direction.y > 0 ? RowUp : RowDown;
        }
    }
    
    /// <summary>
    /// Set direction row configuration at runtime (for different sprite sheets)
    /// </summary>
    public void ConfigureRows(int down, int left, int right, int up)
    {
        RowDown = down;
        RowLeft = left;
        RowRight = right;
        RowUp = up;
    }
}
