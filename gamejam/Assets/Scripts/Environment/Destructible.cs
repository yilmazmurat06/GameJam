using UnityEngine;

/// <summary>
/// Destructible object for Hate Room.
/// Destroyed by Hate mask Shield Bash or Rage Golem charge.
/// Add tag "CrackedWall" or "BrokenFurniture".
/// </summary>
public class Destructible : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool _requireBashToDestroy = true;
    [SerializeField] private GameObject _debrisPrefab;
    [SerializeField] private AudioClip _destroySound;
    
    [Header("Visual")]
    [SerializeField] private Color _crackedColor = new Color(0.7f, 0.6f, 0.5f, 1f);
    
    private SpriteRenderer _sr;
    
    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null)
            _sr.color = _crackedColor;
    }
    
    /// <summary>
    /// Destroy this object (called by abilities or golem).
    /// </summary>
    public void Destroy()
    {
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
        
        Debug.Log($"[Destructible] {gameObject.name} destroyed!");
        Destroy(gameObject);
    }
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!_requireBashToDestroy)
        {
            Destroy();
            return;
        }
        
        // Check if hit by Shield Bash
        HateAbility bash = col.gameObject.GetComponent<HateAbility>();
        if (bash != null && bash.IsActive)
        {
            Destroy();
        }
        
        // Check if hit by Rage Golem
        RageGolemAI golem = col.gameObject.GetComponent<RageGolemAI>();
        if (golem != null)
        {
            Destroy();
        }
    }
}
