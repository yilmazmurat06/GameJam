using UnityEngine;

/// <summary>
/// Iron bars that block passage until opened.
/// Can be linked to pressure plates or other triggers.
/// Used in the Sorrow room.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class IronBars : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool _startsOpen = false;
    [SerializeField] private float _openSpeed = 2f;
    [SerializeField] private float _openHeight = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip _openSound;
    [SerializeField] private AudioClip _closeSound;
    
    private bool _isOpen;
    private Vector3 _closedPosition;
    private Vector3 _openPosition;
    private BoxCollider2D _collider;
    
    public bool IsOpen => _isOpen;
    
    private void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
        _closedPosition = transform.position;
        _openPosition = _closedPosition + new Vector3(0, _openHeight, 0);
        
        _isOpen = _startsOpen;
        if (_startsOpen)
        {
            transform.position = _openPosition;
            _collider.enabled = false;
        }
    }
    
    private void Update()
    {
        // Smoothly move to target position
        Vector3 targetPos = _isOpen ? _openPosition : _closedPosition;
        
        if (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, _openSpeed * Time.deltaTime);
        }
        
        // Enable/disable collider based on position
        _collider.enabled = Vector3.Distance(transform.position, _closedPosition) < 0.1f;
    }
    
    /// <summary>
    /// Open the bars (raise them up).
    /// </summary>
    public void Open()
    {
        if (_isOpen) return;
        
        _isOpen = true;
        Debug.Log("[IronBars] Opening!");
        
        if (_openSound != null)
        {
            AudioSource.PlayClipAtPoint(_openSound, transform.position);
        }
    }
    
    /// <summary>
    /// Close the bars (lower them down).
    /// </summary>
    public void Close()
    {
        if (!_isOpen) return;
        
        _isOpen = false;
        Debug.Log("[IronBars] Closing!");
        
        if (_closeSound != null)
        {
            AudioSource.PlayClipAtPoint(_closeSound, transform.position);
        }
    }
    
    /// <summary>
    /// Toggle the bars open/closed.
    /// </summary>
    public void Toggle()
    {
        if (_isOpen) Close();
        else Open();
    }
}
