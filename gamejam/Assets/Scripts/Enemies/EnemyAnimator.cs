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
    
    public bool HasDirectionalSprites => _allSprites != null && _allSprites.Count >= _columns;

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

    private void Update()
    {
        if (_allSprites == null || _allSprites.Count < _columns)
        {
            // Not enough sprites, fall back to simple flip
            HandleSimpleFlip();
            return;
        }
        
        // Get velocity direction
        Vector2 vel = _rb != null ? _rb.linearVelocity : Vector2.zero;
        bool isMoving = vel.magnitude > 0.15f;
        
        if (isMoving)
        {
            _lastDirection = vel.normalized;
            
            // Determine dominant axis
            if (Mathf.Abs(vel.x) > Mathf.Abs(vel.y))
            {
                // Horizontal movement dominates
                _currentRow = vel.x > 0 ? RowRight : RowLeft;
            }
            else
            {
                // Vertical movement dominates  
                _currentRow = vel.y > 0 ? RowUp : RowDown;
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
        
        // Apply sprite
        int index = (_currentRow * _columns) + _currentFrame;
        if (index >= 0 && index < _allSprites.Count && _sr != null)
        {
            _sr.sprite = _allSprites[index];
        }
    }
    
    private void HandleSimpleFlip()
    {
        if (_rb == null || _sr == null) return;
        
        Vector2 vel = _rb.linearVelocity;
        if (Mathf.Abs(vel.x) > 0.1f)
        {
            _sr.flipX = vel.x < 0;
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
