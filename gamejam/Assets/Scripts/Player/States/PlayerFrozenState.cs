using UnityEngine;

/// <summary>
/// Frozen state - player cannot move (used during cutscenes).
/// Player is completely immobilized until cutscene ends.
/// </summary>
public class PlayerFrozenState : IPlayerState
{
    public void Enter(PlayerController player)
    {
        // Stop all movement
        player.SetHorizontalVelocity(0f);
        player.Rigidbody.linearVelocity = Vector2.zero;
        
        // Could trigger frozen/idle animation
        // player.Animator?.SetBool("IsFrozen", true);
        
        Debug.Log("[PlayerFrozenState] Player frozen for cutscene");
    }
    
    public void Execute(PlayerController player)
    {
        // Do nothing - player is frozen
        // State exit is triggered externally via GameEvents.OnCutsceneEnd
    }
    
    public void Exit(PlayerController player)
    {
        // Re-enable normal behavior
        // player.Animator?.SetBool("IsFrozen", false);
        
        Debug.Log("[PlayerFrozenState] Player unfrozen");
    }
}