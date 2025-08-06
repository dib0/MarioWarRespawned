namespace MarioWarRespawned.Map
{
    /// <summary>
    /// Defines the different types of tiles available in the game
    /// </summary>
    public enum TileType
    {
        Empty = 0,        // No collision, transparent
        Solid = 1,        // Full collision block (like brick)
        Platform = 2,     // One-way platform (can jump through from below)
        Breakable = 3,    // Block that can be destroyed by players
        Coin = 4,         // Collectible coin tile
        Hazard = 5,       // Spikes, lava, etc. - damages players
        Water = 6,        // Water physics area
        Ice = 7,          // Slippery surface
        Conveyor = 8,     // Moving platform surface
        Spring = 9,       // Bouncy surface
        OneWayLeft = 10,  // One-way collision (left only)
        OneWayRight = 11, // One-way collision (right only)
        OneWayUp = 12,    // One-way collision (up only)
        OneWayDown = 13,  // One-way collision (down only)
        Goal = 14,        // Level exit/goal tile
        Checkpoint = 15   // Save checkpoint
    }
}