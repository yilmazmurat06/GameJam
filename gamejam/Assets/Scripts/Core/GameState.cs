/// <summary>
/// Defines all possible game states for the psychological thriller platformer.
/// </summary>
public enum GameState
{
    /// <summary>Main menu (future scope)</summary>
    Menu,
    
    /// <summary>Initial gameplay area - player wakes up</summary>
    Bedroom,
    
    /// <summary>Player frozen, story moment playing</summary>
    Cutscene,
    
    /// <summary>Final gameplay area after following wife</summary>
    Dungeon,
    
    /// <summary>Game is paused</summary>
    Paused
}