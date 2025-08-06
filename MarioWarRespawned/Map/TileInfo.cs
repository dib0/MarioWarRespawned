using Microsoft.Xna.Framework;

namespace MarioWarRespawned.Map
{
    /// <summary>
    /// Information about a specific tile type
    /// </summary>
    public class TileInfo
    {
        public string Name { get; }
        public Color Color { get; }
        public bool IsSolid { get; }
        public bool IsBreakable { get; }
        public bool IsCollectible { get; }

        public TileInfo(string name, Color color, bool isSolid, bool isBreakable, bool isCollectible)
        {
            Name = name;
            Color = color;
            IsSolid = isSolid;
            IsBreakable = isBreakable;
            IsCollectible = isCollectible;
        }
    }
}