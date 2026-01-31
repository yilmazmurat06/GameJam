using UnityEngine;

/// <summary>
/// Idle state - player is stationary (top-down).
/// Transitions to Move when input detected.
/// </summary>
public class PlayerIdleState : IPlayerState
{
    // Animator parameter hashes for performance
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    
    public void Enter(PlayerController player)
    {
        // Stop all movement
        player.SetVelocity(Vector2.zero);
        
        // Set idle animation
        if (player.Animator != null)
        {
            player.Animator.SetBool(IsMovingHash, false);
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
        
        // Check for movement input (8-directional)
        if (player.InputHandler.MoveInput.magnitude > 0.1f)
        {
            player.ChangeState(new PlayerMoveState());
            return;
        }
    }
    
    public void Exit(PlayerController player)
    {
        // Cleanup if needed
    }
}