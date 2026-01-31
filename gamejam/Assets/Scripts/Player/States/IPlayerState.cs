/// <summary>
/// Interface for Player FSM states.
/// Each state handles Enter, Execute (per-frame), and Exit logic.
/// </summary>
public interface IPlayerState
{
    /// <summary>Called when entering this state</summary>
    void Enter(PlayerController player);
    
    /// <summary>Called every frame while in this state</summary>
    void Execute(PlayerController player);
    
    /// <summary>Called when exiting this state</summary>
    void Exit(PlayerController player);
}