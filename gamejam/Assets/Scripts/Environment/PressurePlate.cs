using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Pressure plate that activates when stepped on.
/// Used in the Guilt room.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class PressurePlate : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool _staysPressed = false;
    [SerializeField] private float _pressDepth = 0.1f;
    [SerializeField] private string[] _activatorTags = { "Player", "PushableBlock" };
    
    [Header("Visual")]
    [SerializeField] private Color _pressedColor = new Color(0.5f, 0.5f, 0.3f, 1f);
    [SerializeField] private Color _unpressedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    
    [Header("Audio")]
    [SerializeField] private AudioClip _pressSound;
    [SerializeField] private AudioClip _releaseSound;
    
    [Header("Events")]
    public UnityEvent OnPressed;
    public UnityEvent OnReleased;
    
    private SpriteRenderer _spriteRenderer;
    private bool _isPressed = false;
    private int _objectsOnPlate = 0;
    private Vector3 _originalPosition;
    
    public bool IsPressed => _isPressed;
    
    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.color = _unpressedColor;
        _originalPosition = transform.position;
        
        // Make trigger
        GetComponent<BoxCollider2D>().isTrigger = true;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsActivator(other)) return;
        
        _objectsOnPlate++;
        
        if (!_isPressed)
        {
            Press();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsActivator(other)) return;
        if (_staysPressed && _isPressed) return;
        
        _objectsOnPlate--;
        
        if (_objectsOnPlate <= 0 && _isPressed)
        {
            _objectsOnPlate = 0;
            Release();
        }
    }
    
    private bool IsActivator(Collider2D col)
    {
        foreach (string tag in _activatorTags)
        {
            if (col.CompareTag(tag)) return true;
        }
        return false;
    }
    
    private void Press()
    {
        _isPressed = true;
        
        // Visual feedback
        _spriteRenderer.color = _pressedColor;
        transform.position = _originalPosition - new Vector3(0, _pressDepth, 0);
        
        // Audio
        if (_pressSound != null)
        {
            AudioSource.PlayClipAtPoint(_pressSound, transform.position);
        }
        
        Debug.Log("[PressurePlate] Pressed!");
        OnPressed?.Invoke();
    }
    
    private void Release()
    {
        _isPressed = false;
        
        // Visual feedback
        _spriteRenderer.color = _unpressedColor;
        transform.position = _originalPosition;
        
        // Audio
        if (_releaseSound != null)
        {
            AudioSource.PlayClipAtPoint(_releaseSound, transform.position);
        }
        
        Debug.Log("[PressurePlate] Released!");
        OnReleased?.Invoke();
    }
}
