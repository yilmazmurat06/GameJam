using System;
using UnityEngine;

/// <summary>
/// Singleton GameManager handling global game state.
/// Controls state transitions and broadcasts state changes.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton Instance
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }
    
    // Current State
    [SerializeField] private GameState _currentState = GameState.Bedroom;
    public GameState CurrentState => _currentState;
    
    // State Change Event
    public event Action<GameState, GameState> OnStateChanged;
    
    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void OnEnable()
    {
        // Subscribe to cutscene events
        GameEvents.OnCutsceneStart += HandleCutsceneStart;
        GameEvents.OnCutsceneEnd += HandleCutsceneEnd;
        GameEvents.OnDungeonEntered += HandleDungeonEntered;
    }
    
    private void OnDisable()
    {
        GameEvents.OnCutsceneStart -= HandleCutsceneStart;
        GameEvents.OnCutsceneEnd -= HandleCutsceneEnd;
        GameEvents.OnDungeonEntered -= HandleDungeonEntered;
    }
    
    /// <summary>
    /// Changes the current game state and invokes OnStateChanged event.
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (_currentState == newState) return;
        
        GameState previousState = _currentState;
        _currentState = newState;
        
        Debug.Log($"[GameManager] State changed: {previousState} -> {newState}");
        OnStateChanged?.Invoke(previousState, newState);
    }
    
    /// <summary>
    /// Pauses or unpauses the game.
    /// </summary>
    public void SetPaused(bool paused)
    {
        if (paused)
        {
            Time.timeScale = 0f;
            ChangeState(GameState.Paused);
        }
        else
        {
            Time.timeScale = 1f;
            // Return to previous state - for simplicity, go to Bedroom
            // In a full game, you'd track the previous state
            ChangeState(GameState.Bedroom);
        }
    }
    
    // --- Event Handlers ---
    
    private void HandleCutsceneStart()
    {
        ChangeState(GameState.Cutscene);
    }
    
    private void HandleCutsceneEnd()
    {
        // Return to appropriate state based on context
        // For now, default to Bedroom (can be extended)
        ChangeState(GameState.Bedroom);
    }
    
    private void HandleDungeonEntered()
    {
        ChangeState(GameState.Dungeon);
    }
}