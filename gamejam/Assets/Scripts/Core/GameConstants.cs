using UnityEngine;

/// <summary>
/// Constants for SoulKnight-style game mechanics.
/// Based on the MountPOTATO/SoulKnight reference project.
/// </summary>
public static class GameConstants
{
    // Player Stats (Knight defaults)
    public const int KNIGHT_HP = 6;
    public const int KNIGHT_MP = 200;
    public const int KNIGHT_ARMOR = 7;
    public const float KNIGHT_SPEED = 5f;
    
    // Armor System
    public const float ARMOR_REGEN_DELAY = 3f;
    public const float ARMOR_REGEN_RATE = 0.5f;
    
    // MP System
    public const float MP_REGEN_RATE = 10f;
    public const float MP_REGEN_DELAY = 2f;
    
    // Combat
    public const float INVINCIBILITY_DURATION = 0.5f;
    public const float KNOCKBACK_RECOVERY_TIME = 0.2f;
    
    // Enemy AI
    public const float ENEMY_REACTION_TIME = 0.3f;
    public const float ENEMY_ATTACK_TOKEN_LIMIT = 3; // Max enemies attacking at once
    
    // Weapon Types
    public const float MELEE_BASE_COOLDOWN = 0.5f;
    public const float GUN_BASE_COOLDOWN = 0.3f;
    
    // Directions (matching SoulKnight)
    public const int DIR_NONE = 0;
    public const int DIR_DOWN = 1;
    public const int DIR_RIGHT = 2;
    public const int DIR_UP = 3;
    public const int DIR_LEFT = 4;
    
    /// <summary>
    /// Convert a Vector2 direction to a direction constant.
    /// </summary>
    public static int VectorToDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.01f) return DIR_NONE;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        // Normalize to 0-360
        if (angle < 0) angle += 360;
        
        // Determine direction (45-degree segments)
        if (angle >= 315 || angle < 45) return DIR_RIGHT;
        if (angle >= 45 && angle < 135) return DIR_UP;
        if (angle >= 135 && angle < 225) return DIR_LEFT;
        return DIR_DOWN;
    }
    
    /// <summary>
    /// Convert a direction constant to a normalized Vector2.
    /// </summary>
    public static Vector2 DirectionToVector(int dir)
    {
        return dir switch
        {
            DIR_DOWN => Vector2.down,
            DIR_RIGHT => Vector2.right,
            DIR_UP => Vector2.up,
            DIR_LEFT => Vector2.left,
            _ => Vector2.zero
        };
    }
}
