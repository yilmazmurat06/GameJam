using UnityEngine;

/// <summary>
/// Enemy death state - handles death and cleanup.
/// </summary>
public class EnemyDeathState : IEnemyState
{
    private float _deathTimer = 1f;
    
    public void Enter(EnemyBase enemy)
    {
        // Stop movement
        enemy.SetVelocity(Vector2.zero);
        
        // Disable collider to prevent further interactions
        Collider2D collider = enemy.GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
        
        // Could trigger death animation
        Debug.Log($"[{enemy.EnemyName}] Died!");
    }
    
    public void Execute(EnemyBase enemy)
    {
        _deathTimer -= Time.deltaTime;
        
        if (_deathTimer <= 0)
        {
            // Destroy the enemy
            Object.Destroy(enemy.gameObject);
        }
    }
    
    public void Exit(EnemyBase enemy)
    {
        // Cleanup (won't be called if destroyed)
    }
}
