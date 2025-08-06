using MarioWarRespawned.Configuration;
using MarioWarRespawned.GameStates;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace MarioWarRespawned.Map
{
    /// <summary>
    /// Represents a game map with tiles and metadata
    /// </summary>
    public class GameMap
    {
        public string Name { get; set; } = "Untitled";
        public int Width { get; private set; }
        public int Height { get; private set; }
        public TileType[,] Tiles { get; private set; }
        public List<SpawnPoint> SpawnPoints { get; set; } = new();
        public List<ItemSpawn> ItemSpawns { get; set; } = new();
        public MapMetadata Metadata { get; set; } = new();

        public const int TILE_SIZE = 32;

        public GameMap(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new TileType[width, height];
            InitializeEmpty();
        }

        private void InitializeEmpty()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Tiles[x, y] = TileType.Empty;
                }
            }
        }

        public TileType GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return TileType.Solid; // Treat out-of-bounds as solid

            return Tiles[x, y];
        }

        public void SetTile(int x, int y, TileType tileType)
        {
            if (IsValidPosition(x, y))
            {
                Tiles[x, y] = tileType;
            }
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public Vector2 TileToWorld(int tileX, int tileY)
        {
            return new Vector2(tileX * TILE_SIZE, tileY * TILE_SIZE);
        }

        public Point WorldToTile(Vector2 worldPos)
        {
            return new Point((int)(worldPos.X / TILE_SIZE), (int)(worldPos.Y / TILE_SIZE));
        }

        public Rectangle GetTileBounds(int x, int y)
        {
            return new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
        }

        public bool IsCollisionTile(int x, int y)
        {
            return TileProperties.IsSolid(GetTile(x, y));
        }

        public List<Rectangle> GetCollisionTiles(Rectangle area)
        {
            var tiles = new List<Rectangle>();

            int startX = Math.Max(0, area.Left / TILE_SIZE);
            int endX = Math.Min(Width - 1, area.Right / TILE_SIZE);
            int startY = Math.Max(0, area.Top / TILE_SIZE);
            int endY = Math.Min(Height - 1, area.Bottom / TILE_SIZE);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if (IsCollisionTile(x, y))
                    {
                        tiles.Add(GetTileBounds(x, y));
                    }
                }
            }

            return tiles;
        }

        /// <summary>
        /// Save map to file in simple text format
        /// </summary>
        public void SaveToFile(string filePath)
        {
            try
            {
                var lines = new List<string>
                {
                    $"# Mario War Respawned Map File",
                    $"# Created: {DateTime.Now}",
                    $"Name:{Name}",
                    $"Size:{Width},{Height}",
                    $"Author:{Metadata.Author}",
                    $"Description:{Metadata.Description}",
                    $"Difficulty:{(int)Metadata.Difficulty}",
                    $"Theme:{Metadata.Theme}",
                    "",
                    "# Tile Data (each character represents a tile type)",
                    "Tiles:"
                };

                // Save tiles as compressed format
                for (int y = 0; y < Height; y++)
                {
                    var row = "";
                    for (int x = 0; x < Width; x++)
                    {
                        row += ((int)Tiles[x, y]).ToString("X"); // Hexadecimal for compact storage
                    }
                    lines.Add(row);
                }

                // Save spawn points
                lines.Add("");
                lines.Add("# Player Spawn Points (x,y,playerIndex)");
                lines.Add("Spawns:");
                foreach (var spawn in SpawnPoints)
                {
                    lines.Add($"{spawn.Position.X},{spawn.Position.Y},{spawn.PlayerIndex}");
                }

                // Save item spawns
                lines.Add("");
                lines.Add("# Item Spawn Points (x,y,itemType,respawnTime)");
                lines.Add("Items:");
                foreach (var item in ItemSpawns)
                {
                    lines.Add($"{item.Position.X},{item.Position.Y},{(int)item.ItemType},{item.RespawnTime}");
                }

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save map: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load map from file
        /// </summary>
        public static GameMap LoadFromFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                GameMap map = null;
                string currentSection = "";

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                        continue;

                    if (trimmed.EndsWith(":"))
                    {
                        currentSection = trimmed.TrimEnd(':');
                        continue;
                    }

                    var parts = trimmed.Split(':');

                    switch (parts[0])
                    {
                        case "Name":
                            // Map will be created when we read Size
                            break;
                        case "Size":
                            var sizeParts = parts[1].Split(',');
                            var width = int.Parse(sizeParts[0]);
                            var height = int.Parse(sizeParts[1]);
                            map = new GameMap(width, height);
                            break;
                        case "Author":
                            if (map != null) map.Metadata.Author = parts[1];
                            break;
                        case "Description":
                            if (map != null) map.Metadata.Description = parts[1];
                            break;
                        case "Difficulty":
                            if (map != null) map.Metadata.Difficulty = (DifficultyLevel)int.Parse(parts[1]);
                            break;
                        case "Theme":
                            if (map != null) map.Metadata.Theme = parts[1];
                            break;
                    }

                    // Handle sections
                    if (currentSection == "Tiles" && map != null)
                    {
                        var tileData = lines.Skip(Array.IndexOf(lines, "Tiles:") + 1)
                                          .Take(map.Height)
                                          .ToArray();

                        for (int y = 0; y < Math.Min(map.Height, tileData.Length); y++)
                        {
                            var row = tileData[y].Trim();
                            for (int x = 0; x < Math.Min(map.Width, row.Length); x++)
                            {
                                if (int.TryParse(row[x].ToString(), NumberStyles.HexNumber, null, out int tileType))
                                {
                                    map.Tiles[x, y] = (TileType)tileType;
                                }
                            }
                        }
                        break; // Exit after processing tiles
                    }
                }

                // Load spawn points and items (simplified for now)
                LoadSpawnData(map, lines);

                return map ?? new GameMap(64, 24); // Default map if loading fails
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load map: {ex.Message}", ex);
            }
        }

        private static void LoadSpawnData(GameMap map, string[] lines)
        {
            bool inSpawns = false;
            bool inItems = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed == "Spawns:")
                {
                    inSpawns = true;
                    inItems = false;
                    continue;
                }
                else if (trimmed == "Items:")
                {
                    inSpawns = false;
                    inItems = true;
                    continue;
                }
                else if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                {
                    continue;
                }

                if (inSpawns)
                {
                    var parts = trimmed.Split(',');
                    if (parts.Length >= 3)
                    {
                        map.SpawnPoints.Add(new SpawnPoint
                        {
                            Position = new Vector2(float.Parse(parts[0]), float.Parse(parts[1])),
                            PlayerIndex = int.Parse(parts[2])
                        });
                    }
                }
                else if (inItems)
                {
                    var parts = trimmed.Split(',');
                    if (parts.Length >= 4)
                    {
                        map.ItemSpawns.Add(new ItemSpawn
                        {
                            Position = new Vector2(float.Parse(parts[0]), float.Parse(parts[1])),
                            ItemType = (ItemType)int.Parse(parts[2]),
                            RespawnTime = float.Parse(parts[3])
                        });
                    }
                }
            }
        }
    }
}