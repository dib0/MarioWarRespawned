using Microsoft.Xna.Framework;

namespace MarioWarRespawned.Map
{
    /// <summary>
    /// Properties and behavior definitions for each tile type
    /// </summary>
    public static class TileProperties
    {
        private static readonly Dictionary<TileType, TileInfo> _tileInfo = new()
        {
            { TileType.Empty, new TileInfo("Empty", Color.Transparent, false, false, false) },
            { TileType.Solid, new TileInfo("Solid Block", Color.Brown, true, false, false) },
            { TileType.Platform, new TileInfo("Platform", Color.Orange, true, false, false) },
            { TileType.Breakable, new TileInfo("Breakable Block", Color.Yellow, true, true, false) },
            { TileType.Coin, new TileInfo("Coin", Color.Gold, false, false, true) },
            { TileType.Hazard, new TileInfo("Hazard", Color.Red, false, false, false) },
            { TileType.Water, new TileInfo("Water", Color.Blue, false, false, false) },
            { TileType.Ice, new TileInfo("Ice", Color.LightBlue, true, false, false) },
            { TileType.Conveyor, new TileInfo("Conveyor Belt", Color.Gray, true, false, false) },
            { TileType.Spring, new TileInfo("Spring", Color.Green, true, false, false) },
            { TileType.OneWayLeft, new TileInfo("One-Way Left", Color.Purple, true, false, false) },
            { TileType.OneWayRight, new TileInfo("One-Way Right", Color.Purple, true, false, false) },
            { TileType.OneWayUp, new TileInfo("One-Way Up", Color.Purple, true, false, false) },
            { TileType.OneWayDown, new TileInfo("One-Way Down", Color.Purple, true, false, false) },
            { TileType.Goal, new TileInfo("Goal", Color.Lime, false, false, true) },
            { TileType.Checkpoint, new TileInfo("Checkpoint", Color.Cyan, false, false, true) }
        };

        public static TileInfo GetInfo(TileType tileType)
        {
            return _tileInfo.GetValueOrDefault(tileType, _tileInfo[TileType.Empty]);
        }

        public static bool IsSolid(TileType tileType)
        {
            return GetInfo(tileType).IsSolid;
        }

        public static bool IsBreakable(TileType tileType)
        {
            return GetInfo(tileType).IsBreakable;
        }

        public static bool IsCollectible(TileType tileType)
        {
            return GetInfo(tileType).IsCollectible;
        }

        public static Color GetColor(TileType tileType)
        {
            return GetInfo(tileType).Color;
        }

        public static string GetName(TileType tileType)
        {
            return GetInfo(tileType).Name;
        }

        public static TileType[] GetAvailableTiles()
        {
            return _tileInfo.Keys.ToArray();
        }

        public static TileType[] GetEditorTiles()
        {
            // Return tiles that should be available in the level editor
            return
            [
                TileType.Empty,
                TileType.Solid,
                TileType.Platform,
                TileType.Breakable,
                TileType.Coin,
                TileType.Hazard,
                TileType.Water,
                TileType.Ice,
                TileType.Spring,
                TileType.Goal
            ];
        }
    }
}