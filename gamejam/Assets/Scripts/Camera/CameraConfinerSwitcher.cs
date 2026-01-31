using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Switches Cinemachine confiner bounds when the player moves between rooms.
/// Works with CinemachineConfiner2D component.
/// </summary>
public class CameraConfinerSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera _virtualCamera;
    [SerializeField] private CinemachineConfiner2D _confiner;
    
    [Header("Settings")]
    [SerializeField] private float _transitionDuration = 0.5f;
    
    // Current bounds
    private Collider2D _currentBounds;
    
    private void Awake()
    {
        if (_virtualCamera == null)
            _virtualCamera = GetComponent<CinemachineCamera>();
        
        if (_confiner == null && _virtualCamera != null)
            _confiner = _virtualCamera.GetComponent<CinemachineConfiner2D>();
    }
    
    /// <summary>
    /// Switches to new confiner bounds.
    /// </summary>
    public void SwitchConfiner(Collider2D newBounds)
    {
        if (newBounds == null || newBounds == _currentBounds) return;
        
        Debug.Log($"[CameraConfinerSwitcher] Switching to bounds: {newBounds.name}");
        
        _currentBounds = newBounds;
        
        if (_confiner != null)
        {
            _confiner.BoundingShape2D = newBounds;
            _confiner.InvalidateBoundingShapeCache();
        }
    }
    
    /// <summary>
    /// Gets the current bounds collider.
    /// </summary>
    public Collider2D GetCurrentBounds()
    {
        return _currentBounds;
    }
}