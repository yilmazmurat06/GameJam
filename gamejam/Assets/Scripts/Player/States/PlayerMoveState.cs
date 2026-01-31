using UnityEngine;

/// <summary>
/// Move state - player is moving horizontally.
/// Transitions to Idle when no input, Jump when jump pressed.
/// </summary>
public class PlayerMoveState : IPlayerState
{
    public void Enter(PlayerController player)
    {
        // Could trigger walk animation
        // player.Animator?.SetBool("IsMoving", true);
    }
    
    public void Execute(PlayerController player)
    {
        float horizontalInput = player.InputHandler.HorizontalInput;
        
        // Apply horizontal movement
        player.SetHorizontalVelocity(horizontalInput * player.MoveSpeed);
        
        // Flip sprite based on direction
        if (horizontalInput != 0)
        {
            player.SetFacingDirection(horizontalInput > 0);
        }
        
        // Check for jump
        if (player.InputHandler.JumpPressed && player.IsGrounded)
        {
            player.ChangeState(new PlayerJumpState());
            return;
        }
        
        // Return to idle if no input
        if (Mathf.Abs(horizontalInput) < 0.1f)
        {
            player.ChangeState(new PlayerIdleState());
            return;
        }
    }
    
    public void Exit(PlayerController player)
    {
        // Cleanup if needed
    }
}