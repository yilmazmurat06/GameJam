using UnityEngine;

/// <summary>
/// Move state - player is moving in 8 directions (top-down).
/// Transitions to Idle when no input.
/// Updates animator parameters for directional animations.
/// </summary>
public class PlayerMoveState : IPlayerState
{
    // Animator parameter hashes for performance
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    
    public void Enter(PlayerController player)
    {
        // Set moving flag for animator
        if (player.Animator != null)
        {
            player.Animator.SetBool(IsMovingHash, true);
        }
    }
    
    public void Execute(PlayerController player)
    {
        // Check for attack input
        if (player.InputHandler.AttackPressed)
        {
            player.ChangeState(new PlayerAttackState());
            return;
        }
        
        Vector2 moveInput = player.InputHandler.MoveInput;
        
        // Apply 8-directional movement
        player.SetVelocity(moveInput * player.MoveSpeed);
        
        // Update animator blend tree parameters with normalized direction
        // Only update if moving to preserve last direction for idle state
        if (player.Animator != null && moveInput.magnitude > 0.01f)
        {
            // Normalize the vector we send to the animator for clean directional states
            // This prevents "middle" states in the blend tree
            Vector2 animDir = moveInput.normalized;
            player.Animator.SetFloat(MoveXHash, animDir.x);
            player.Animator.SetFloat(MoveYHash, animDir.y);
        }
        
        // Return to idle if no input
        if (moveInput.magnitude < 0.1f)
        {
            player.ChangeState(new PlayerIdleState());
            return;
        }
    }
    
    public void Exit(PlayerController player)
    {
        // Clear moving flag
        if (player.Animator != null)
        {
            player.Animator.SetBool(IsMovingHash, false);
        }
    }
}