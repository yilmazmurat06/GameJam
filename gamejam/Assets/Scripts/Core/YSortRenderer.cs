using UnityEngine;

/// <summary>
/// Dynamically updates SpriteRenderer.sortingOrder based on Y position.
/// Lower Y = higher sorting order (renders in front).
/// Attach to Player and all environment objects that need depth sorting.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class YSortRenderer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _sortingOrderBase = 5000;
    [SerializeField] private float _precision = 10f;
    [SerializeField] private bool _runOnlyOnce = false;
    
    [Header("Offset")]
    [Tooltip("Y offset for pivot point adjustment")]
    [SerializeField] private float _yOffset = 0f;
    
    private SpriteRenderer _spriteRenderer;
    private bool _hasRun = false;
    
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void LateUpdate()
    {
        if (_runOnlyOnce && _hasRun) return;
        
        // Calculate sorting order: lower Y = higher order (in front)
        float adjustedY = transform.position.y + _yOffset;
        _spriteRenderer.sortingOrder = (int)(_sortingOrderBase - adjustedY * _precision);
        
        _hasRun = true;
    }
    
    /// <summary>
    /// Force update sorting order (useful after teleporting).
    /// </summary>
    public void ForceUpdate()
    {
        _hasRun = false;
    }
}
