using UnityEngine;

/// <summary>
/// Smooth camera follow for room-based navigation.
/// Snaps to room bounds when entering camera zones.
/// </summary>
public class RoomCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;
    
    [Header("Following")]
    [SerializeField] private float _followSpeed = 5f;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);
    
    [Header("Room Bounds")]
    [SerializeField] private bool _useBounds = true;
    [SerializeField] private float _boundsPadding = 2f;
    
    private Bounds _currentBounds;
    private bool _hasBounds;
    private Camera _camera;
    
    private void Start()
    {
        _camera = GetComponent<Camera>();
        
        // Find player if not assigned
        if (_target == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null) _target = player.transform;
        }
    }
    
    private void LateUpdate()
    {
        if (_target == null) return;
        
        // Calculate target position
        Vector3 targetPos = _target.position + _offset;
        
        // Clamp to bounds if enabled
        if (_useBounds && _hasBounds)
        {
            targetPos = ClampToBounds(targetPos);
        }
        
        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPos, _followSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Set new room bounds. Called by CameraZoneTrigger.
    /// </summary>
    public void SetBounds(Collider2D boundsCollider)
    {
        if (boundsCollider != null)
        {
            _currentBounds = boundsCollider.bounds;
            _hasBounds = true;
            Debug.Log($"[RoomCamera] Set bounds to {_currentBounds}");
        }
    }
    
    /// <summary>
    /// Set bounds from a Vector2 size at a position.
    /// </summary>
    public void SetBounds(Vector3 center, Vector2 size)
    {
        _currentBounds = new Bounds(center, new Vector3(size.x, size.y, 10f));
        _hasBounds = true;
    }
    
    /// <summary>
    /// Disable bounds clamping.
    /// </summary>
    public void ClearBounds()
    {
        _hasBounds = false;
    }
    
    private Vector3 ClampToBounds(Vector3 pos)
    {
        if (_camera == null) return pos;
        
        // Calculate camera half-size
        float camHeight = _camera.orthographicSize;
        float camWidth = camHeight * _camera.aspect;
        
        // Add padding
        float minX = _currentBounds.min.x + camWidth - _boundsPadding;
        float maxX = _currentBounds.max.x - camWidth + _boundsPadding;
        float minY = _currentBounds.min.y + camHeight - _boundsPadding;
        float maxY = _currentBounds.max.y - camHeight + _boundsPadding;
        
        // Handle rooms smaller than camera view
        if (minX > maxX) minX = maxX = _currentBounds.center.x;
        if (minY > maxY) minY = maxY = _currentBounds.center.y;
        
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        
        return pos;
    }
}
