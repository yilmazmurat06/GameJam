using UnityEngine;

/// <summary>
/// Idle state - player is stationary on ground.
/// Transitions to Move when input detected, Jump when jump pressed.
/// </summary>
public class PlayerIdleState : IPlayerState
{
    public void Enter(PlayerController player)
    {
        // Stop horizontal movement
        player.SetHorizontalVelocity(0f);
        
        // Could trigger idle animation here
        // player.Animator?.SetBool("IsMoving", false);
    }
    
    public void Execute(PlayerController player)
    {
        // Check for movement input
        if (Mathf.Abs(player.InputHandler.HorizontalInput) > 0.1f)
        {
            player.ChangeState(new PlayerMoveState());
            return;
        }
        
        // Check for jump
        if (player.InputHandler.JumpPressed && player.IsGrounded)
        {
            player.ChangeState(new PlayerJumpState());
            return;
        }
    }
    
    public void Exit(PlayerController player)
    {
        // Cleanup if needed
    }
}