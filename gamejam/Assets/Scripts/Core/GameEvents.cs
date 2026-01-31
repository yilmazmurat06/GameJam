using System;

/// <summary>
/// Static event hub for decoupled communication between game systems.
/// Avoids tight coupling between Player, NPCs, Camera, and GameManager.
/// </summary>
public static class GameEvents
{
    // Player Events
    public static event Action<IInteractable> OnPlayerInteract;
    public static event Action OnPlayerDied;
    
    // Cutscene Events
    public static event Action OnCutsceneStart;
    public static event Action OnCutsceneEnd;
    
    // Room/Level Events
    public static event Action<string> OnRoomTransition;
    public static event Action OnDungeonEntered;
    
    // Wife NPC Events
    public static event Action OnWifeMaskRevealed;
    public static event Action OnWifeLeftRoom;
    
    // --- Invoke Methods ---
    
    public static void TriggerPlayerInteract(IInteractable interactable)
    {
        OnPlayerInteract?.Invoke(interactable);
    }
    
    public static void TriggerPlayerDied()
    {
        OnPlayerDied?.Invoke();
    }
    
    public static void TriggerCutsceneStart()
    {
        OnCutsceneStart?.Invoke();
    }
    
    public static void TriggerCutsceneEnd()
    {
        OnCutsceneEnd?.Invoke();
    }
    
    public static void TriggerRoomTransition(string roomName)
    {
        OnRoomTransition?.Invoke(roomName);
    }
    
    public static void TriggerDungeonEntered()
    {
        OnDungeonEntered?.Invoke();
    }
    
    public static void TriggerWifeMaskRevealed()
    {
        OnWifeMaskRevealed?.Invoke();
    }
    
    public static void TriggerWifeLeftRoom()
    {
        OnWifeLeftRoom?.Invoke();
    }
}