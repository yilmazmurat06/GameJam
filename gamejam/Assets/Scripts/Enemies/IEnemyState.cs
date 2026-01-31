/// <summary>
/// Interface for enemy AI states.
/// Mirrors the player state pattern for consistency.
/// </summary>
public interface IEnemyState
{
    void Enter(EnemyBase enemy);
    void Execute(EnemyBase enemy);
    void Exit(EnemyBase enemy);
}
