using UnityEngine;

/// <summary>
/// Heavy block that can be pushed by the player.
/// Used in Guilt room puzzles.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PushableBlock : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _pushResistance = 5f;
    [SerializeField] private float _maxSpeed = 3f;
    [SerializeField] private float _friction = 0.9f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip _pushSound;
    
    private Rigidbody2D _rb;
    private bool _isBeingPushed = false;
    private AudioSource _audioSource;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.linearDamping = _friction;
        _rb.mass = _pushResistance;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Tag for pressure plate detection
        gameObject.tag = "PushableBlock";
        
        // Audio source for looping push sound
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.clip = _pushSound;
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
    }
    
    private void Update()
    {
        // Clamp velocity
        if (_rb.linearVelocity.magnitude > _maxSpeed)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * _maxSpeed;
        }
        
        // Handle push sound
        bool isMoving = _rb.linearVelocity.magnitude > 0.1f;
        if (isMoving && !_audioSource.isPlaying && _pushSound != null)
        {
            _audioSource.Play();
        }
        else if (!isMoving && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if player is pushing
        if (collision.gameObject.CompareTag("Player"))
        {
            _isBeingPushed = true;
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _isBeingPushed = false;
        }
    }
}
