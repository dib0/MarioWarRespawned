using Microsoft.Xna.Framework;

namespace MarioWarRespawned.Map
{
    /// <summary>
    /// Represents an item spawn point in a map
    /// </summary>
    public class ItemSpawn
    {
        public Vector2 Position { get; set; }
        public ItemType ItemType { get; set; }
        public float RespawnTime { get; set; } = 10f;
        public bool IsActive { get; set; } = true;
    }
}