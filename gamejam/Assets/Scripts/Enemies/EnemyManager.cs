using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int _maxSimultaneousAttacks = 2;
    
    // Trackers
    private Transform _player;
    private HashSet<EnemyBase> _attackingEnemies = new HashSet<EnemyBase>();
    private List<EnemyBase> _allEnemies = new List<EnemyBase>();

    public Transform Player => _player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Update()
    {
        if (_player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }
        
        // Cleanup nulls
        _attackingEnemies.RemoveWhere(e => e == null);
        _allEnemies.RemoveAll(e => e == null);
    }
    
    public void RegisterEnemy(EnemyBase enemy)
    {
        if (!_allEnemies.Contains(enemy))
            _allEnemies.Add(enemy);
    }
    
    public void UnregisterEnemy(EnemyBase enemy)
    {
        _allEnemies.Remove(enemy);
        ReleaseAttackToken(enemy);
    }

    /// <summary>
    /// Try to get permission to attack.
    /// </summary>
    public bool RequestAttackToken(EnemyBase enemy)
    {
        if (_attackingEnemies.Contains(enemy)) return true; // Already has one

        if (_attackingEnemies.Count < _maxSimultaneousAttacks)
        {
            _attackingEnemies.Add(enemy);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Release permission to attack (e.g. when backing off or dying).
    /// </summary>
    public void ReleaseAttackToken(EnemyBase enemy)
    {
        if (_attackingEnemies.Contains(enemy))
        {
            _attackingEnemies.Remove(enemy);
        }
    }
    
    /// <summary>
    /// Get 'separation' vector to avoid crowding.
    /// Returns a vector away from nearby enemies.
    /// </summary>
    public Vector2 GetSeparationVector(EnemyBase subject, float radius = 1.0f)
    {
        Vector2 separation = Vector2.zero;
        int count = 0;
        
        foreach (var other in _allEnemies)
        {
            if (other == subject || other == null) continue;
            
            float dist = Vector2.Distance(subject.transform.position, other.transform.position);
            if (dist < radius)
            {
                // Push away
                Vector2 dir = (Vector2)subject.transform.position - (Vector2)other.transform.position;
                separation += dir.normalized / dist; // Weight by distance
                count++;
            }
        }
        
        return count > 0 ? separation.normalized : Vector2.zero;
    }
}
