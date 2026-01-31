using UnityEngine;

/// <summary>
/// Jump state - player is in the air after jumping.
/// Transitions to Idle or Move when landing.
/// </summary>
public class PlayerJumpState : IPlayerState
{
    private bool _hasAppliedForce = false;
    
    public void Enter(PlayerController player)
    {
        // Apply upward impulse
        player.ApplyJumpForce();
        _hasAppliedForce = true;
        
        // Could trigger jump animation
        // player.Animator?.SetTrigger("Jump");
    }
    
    public void Execute(PlayerController player)
    {
        // Allow air control
        float horizontalInput = player.InputHandler.HorizontalInput;
        player.SetHorizontalVelocity(horizontalInput * player.MoveSpeed * player.AirControlMultiplier);
        
        // Flip sprite
        if (horizontalInput != 0)
        {
            player.SetFacingDirection(horizontalInput > 0);
        }
        
        // Check if landed
        if (player.IsGrounded && _hasAppliedForce && player.Rigidbody.linearVelocity.y <= 0)
        {
            // Transition based on input
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                player.ChangeState(new PlayerMoveState());
            }
            else
            {
                player.ChangeState(new PlayerIdleState());
            }
        }
    }
    
    public void Exit(PlayerController player)
    {
        // Could trigger land animation
        // player.Animator?.SetTrigger("Land");
    }
}