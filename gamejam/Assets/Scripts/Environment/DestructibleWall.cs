using UnityEngine;

/// <summary>
/// Destructible wall that can be damaged and destroyed.
/// Used in the Hate room.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class DestructibleWall : MonoBehaviour, IDamageable
{
    [Header("Settings")]
    [SerializeField] private float _maxHealth = 30f;
    [SerializeField] private float _currentHealth;
    
    [Header("Effects")]
    [SerializeField] private GameObject _debrisPrefab;
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _destroySound;
    
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    
    // IDamageable interface implementation
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public bool IsAlive => _currentHealth > 0;
    
    private void Start()
    {
        _currentHealth = _maxHealth;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
        
        // Tag for RageGolem detection
        gameObject.tag = "CrackedWall";
    }

    
    public void TakeDamage(DamageInfo damageInfo)
    {
        _currentHealth -= damageInfo.Amount;
        
        // Visual feedback
        StartCoroutine(FlashRed());
        
        Debug.Log($"[DestructibleWall] Took {damageInfo.Amount} damage. Health: {_currentHealth}/{_maxHealth}");
        
        if (_currentHealth <= 0)
        {
            Destroy();
        }
    }
    
    private System.Collections.IEnumerator FlashRed()
    {
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.color = _originalColor;
    }
    
    private void Destroy()
    {
        Debug.Log("[DestructibleWall] Destroyed!");
        
        // Spawn debris
        if (_debrisPrefab != null)
        {
            Instantiate(_debrisPrefab, transform.position, Quaternion.identity);
        }
        
        // Play sound
        if (_destroySound != null)
        {
            AudioSource.PlayClipAtPoint(_destroySound, transform.position);
        }
        
        Destroy(gameObject);
    }
}
