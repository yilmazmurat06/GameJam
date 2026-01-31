/// <summary>
/// Interface for any object that can be interacted with by the player.
/// Implement this on NPCs, doors, items, etc.
/// </summary>
public interface IInteractable
{
    /// <summary>Called when player interacts with this object</summary>
    void OnInteract(PlayerController player);
    
    /// <summary>Whether this object can currently be interacted with</summary>
    bool CanInteract { get; }
    
    /// <summary>Text shown to player when in range (e.g., "Press E to talk")</summary>
    string InteractionPrompt { get; }
}