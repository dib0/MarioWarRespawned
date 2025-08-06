using MarioWarRespawned.Configuration;
using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using MarioWarRespawned.Map;
using MarioWarRespawned.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MarioWarRespawned.GameStates
{
    public class LevelEditorState : IGameState
    {
        private readonly Game _game;
        private readonly ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;
        private readonly GameStateManager _stateManager;

        private SpriteFont _font;
        private SpriteFont _smallFont;
        private Camera2D _camera;
        private KeyboardState _previousKeyboard;
        private MouseState _previousMouse;

        // Editor state
        private EditorMode _currentMode = EditorMode.Paint;
        private TileType _selectedTile = TileType.Solid;
        private bool _isGridVisible = true;
        private string _currentMapName = "newmap";
        private bool _showHelp = false;

        // Map data
        private const int MAP_WIDTH = 64;
        private const int MAP_HEIGHT = 24;
        private const int TILE_SIZE = 32;
        private TileType[,] _mapTiles = new TileType[MAP_WIDTH, MAP_HEIGHT];
        private List<SpawnPoint> _spawnPoints = new();

        // Textures
        private Texture2D _tileTextures;
        private Texture2D _gridTexture;
        private Dictionary<TileType, Rectangle> _tileSourceRects = new();

        // UI
        private readonly string[] _toolNames = { "Paint", "Erase", "Spawn", "Select" };
        private readonly TileType[] _availableTiles =
        {
            TileType.Empty, TileType.Solid, TileType.Platform,
            TileType.Breakable, TileType.Coin, TileType.Hazard
        };

        public LevelEditorState(Game game, ContentManager contentManager, InputManager inputManager,
                              AudioManager audioManager, GameStateManager stateManager)
        {
            _game = game;
            _contentManager = contentManager;
            _inputManager = inputManager;
            _audioManager = audioManager;
            _stateManager = stateManager;
        }

        public void Initialize()
        {
            _font = _contentManager.GetFont("Menu");
            _smallFont = _contentManager.GetFont("UI") ?? _font;
            _camera = new Camera2D();

            // Initialize map with empty tiles
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                for (int y = 0; y < MAP_HEIGHT; y++)
                {
                    _mapTiles[x, y] = TileType.Empty;
                }
            }

            // Add ground line
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                _mapTiles[x, MAP_HEIGHT - 3] = TileType.Solid;
            }

            // Setup tile textures and source rectangles
            SetupTileTextures();

            // Add default spawn points
            _spawnPoints.Add(new SpawnPoint { Position = new Vector2(2 * TILE_SIZE, (MAP_HEIGHT - 4) * TILE_SIZE), PlayerIndex = 0 });
            _spawnPoints.Add(new SpawnPoint { Position = new Vector2(6 * TILE_SIZE, (MAP_HEIGHT - 4) * TILE_SIZE), PlayerIndex = 1 });

            _previousKeyboard = Keyboard.GetState();
            _previousMouse = Mouse.GetState();

            _audioManager.PlayMusic("menu_theme");
        }

        private void SetupTileTextures()
        {
            // Create simple colored textures for tiles
            _tileTextures = new Texture2D(_game.GraphicsDevice, TILE_SIZE * 6, TILE_SIZE);
            var colors = new Color[TILE_SIZE * 6 * TILE_SIZE];

            var tileColors = new Dictionary<TileType, Color>
            {
                { TileType.Empty, Color.Transparent },
                { TileType.Solid, Color.Brown },
                { TileType.Platform, Color.Orange },
                { TileType.Breakable, Color.Yellow },
                { TileType.Coin, Color.Gold },
                { TileType.Hazard, Color.Red }
            };

            int tileIndex = 0;
            foreach (var tileType in _availableTiles)
            {
                _tileSourceRects[tileType] = new Rectangle(tileIndex * TILE_SIZE, 0, TILE_SIZE, TILE_SIZE);

                var color = tileColors[tileType];
                for (int y = 0; y < TILE_SIZE; y++)
                {
                    for (int x = 0; x < TILE_SIZE; x++)
                    {
                        int index = (y * TILE_SIZE * 6) + (tileIndex * TILE_SIZE) + x;
                        colors[index] = color;
                    }
                }
                tileIndex++;
            }

            _tileTextures.SetData(colors);

            // Create grid texture
            _gridTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
            _gridTexture.SetData(new[] { Color.Gray });
        }

        public void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();

            // Handle keyboard input
            HandleKeyboardInput(keyboard);

            // Handle mouse input
            HandleMouseInput(mouse);

            // Update camera
            UpdateCamera(keyboard, gameTime);

            _previousKeyboard = keyboard;
            _previousMouse = mouse;
        }

        private void HandleKeyboardInput(KeyboardState keyboard)
        {
            // Mode switching
            if (keyboard.IsKeyDown(Keys.D1) && !_previousKeyboard.IsKeyDown(Keys.D1))
                _currentMode = EditorMode.Paint;
            else if (keyboard.IsKeyDown(Keys.D2) && !_previousKeyboard.IsKeyDown(Keys.D2))
                _currentMode = EditorMode.Erase;
            else if (keyboard.IsKeyDown(Keys.D3) && !_previousKeyboard.IsKeyDown(Keys.D3))
                _currentMode = EditorMode.Spawn;
            else if (keyboard.IsKeyDown(Keys.D4) && !_previousKeyboard.IsKeyDown(Keys.D4))
                _currentMode = EditorMode.Select;

            // Tile selection
            if (keyboard.IsKeyDown(Keys.Q) && !_previousKeyboard.IsKeyDown(Keys.Q))
                CycleTileSelection(-1);
            else if (keyboard.IsKeyDown(Keys.E) && !_previousKeyboard.IsKeyDown(Keys.E))
                CycleTileSelection(1);

            // Grid toggle
            if (keyboard.IsKeyDown(Keys.G) && !_previousKeyboard.IsKeyDown(Keys.G))
                _isGridVisible = !_isGridVisible;

            // Help toggle
            if (keyboard.IsKeyDown(Keys.H) && !_previousKeyboard.IsKeyDown(Keys.H))
                _showHelp = !_showHelp;

            // Save/Load
            if (keyboard.IsKeyDown(Keys.LeftControl))
            {
                if (keyboard.IsKeyDown(Keys.S) && !_previousKeyboard.IsKeyDown(Keys.S))
                    SaveMap();
                else if (keyboard.IsKeyDown(Keys.O) && !_previousKeyboard.IsKeyDown(Keys.O))
                    LoadMap();
                else if (keyboard.IsKeyDown(Keys.N) && !_previousKeyboard.IsKeyDown(Keys.N))
                    NewMap();
            }

            // Test map
            if (keyboard.IsKeyDown(Keys.F5) && !_previousKeyboard.IsKeyDown(Keys.F5))
                TestMap();

            // Back to main menu
            if (keyboard.IsKeyDown(Keys.Escape) && !_previousKeyboard.IsKeyDown(Keys.Escape))
                _stateManager.ChangeState(new MainMenuState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
        }

        private void HandleMouseInput(MouseState mouse)
        {
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                var worldPos = GetWorldPosition(new Vector2(mouse.X, mouse.Y));
                var tileX = (int)(worldPos.X / TILE_SIZE);
                var tileY = (int)(worldPos.Y / TILE_SIZE);

                if (IsValidTilePosition(tileX, tileY))
                {
                    switch (_currentMode)
                    {
                        case EditorMode.Paint:
                            _mapTiles[tileX, tileY] = _selectedTile;
                            break;
                        case EditorMode.Erase:
                            _mapTiles[tileX, tileY] = TileType.Empty;
                            break;
                        case EditorMode.Spawn:
                            PlaceSpawnPoint(worldPos);
                            break;
                    }
                }
            }

            if (mouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released)
            {
                var worldPos = GetWorldPosition(new Vector2(mouse.X, mouse.Y));
                var tileX = (int)(worldPos.X / TILE_SIZE);
                var tileY = (int)(worldPos.Y / TILE_SIZE);

                if (IsValidTilePosition(tileX, tileY))
                {
                    // Sample tile under cursor
                    _selectedTile = _mapTiles[tileX, tileY];
                }
            }
        }

        private void UpdateCamera(KeyboardState keyboard, GameTime gameTime)
        {
            var cameraSpeed = 300f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboard.IsKeyDown(Keys.LeftShift)) cameraSpeed *= 2;

            var movement = Vector2.Zero;
            if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A)) movement.X -= cameraSpeed;
            if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D)) movement.X += cameraSpeed;
            if (keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W)) movement.Y -= cameraSpeed;
            if (keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S)) movement.Y += cameraSpeed;

            _camera.Position += movement;

            // Clamp camera to map bounds
            _camera.Position = Vector2.Clamp(_camera.Position,
                Vector2.Zero,
                new Vector2(MAP_WIDTH * TILE_SIZE - 1280, MAP_HEIGHT * TILE_SIZE - 720));
        }

        private Vector2 GetWorldPosition(Vector2 screenPos)
        {
            return screenPos + _camera.Position;
        }

        private bool IsValidTilePosition(int x, int y)
        {
            return x >= 0 && x < MAP_WIDTH && y >= 0 && y < MAP_HEIGHT;
        }

        private void CycleTileSelection(int direction)
        {
            var currentIndex = Array.IndexOf(_availableTiles, _selectedTile);
            currentIndex = (currentIndex + direction + _availableTiles.Length) % _availableTiles.Length;
            _selectedTile = _availableTiles[currentIndex];
        }

        private void PlaceSpawnPoint(Vector2 worldPos)
        {
            var newSpawn = new SpawnPoint
            {
                Position = new Vector2((int)(worldPos.X / TILE_SIZE) * TILE_SIZE, (int)(worldPos.Y / TILE_SIZE) * TILE_SIZE),
                PlayerIndex = _spawnPoints.Count % 4
            };

            // Remove existing spawn at this position
            _spawnPoints.RemoveAll(sp => Vector2.Distance(sp.Position, newSpawn.Position) < TILE_SIZE / 2);
            _spawnPoints.Add(newSpawn);
        }

        private void SaveMap()
        {
            try
            {
                // Simple text-based save format
                var lines = new List<string>();
                lines.Add($"MapName:{_currentMapName}");
                lines.Add($"Size:{MAP_WIDTH},{MAP_HEIGHT}");

                // Save tiles
                lines.Add("Tiles:");
                for (int y = 0; y < MAP_HEIGHT; y++)
                {
                    var row = "";
                    for (int x = 0; x < MAP_WIDTH; x++)
                    {
                        row += ((int)_mapTiles[x, y]).ToString();
                    }
                    lines.Add(row);
                }

                // Save spawn points
                lines.Add("Spawns:");
                foreach (var spawn in _spawnPoints)
                {
                    lines.Add($"{spawn.Position.X},{spawn.Position.Y},{spawn.PlayerIndex}");
                }

                var mapPath = Path.Combine("Content", "Maps", $"{_currentMapName}.map");
                Directory.CreateDirectory(Path.GetDirectoryName(mapPath));
                File.WriteAllLines(mapPath, lines);

                _audioManager.PlaySound("menu_select");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save map: {ex.Message}");
            }
        }

        private void LoadMap()
        {
            // Simplified - in a full implementation you'd show a file dialog
            _audioManager.PlaySound("menu_select");
        }

        private void NewMap()
        {
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                for (int y = 0; y < MAP_HEIGHT; y++)
                {
                    _mapTiles[x, y] = TileType.Empty;
                }
            }

            _spawnPoints.Clear();
            _audioManager.PlaySound("menu_select");
        }

        private void TestMap()
        {
            // Create a test game with the current map
            var settings = new GameSettings
            {
                GameMode = GameModeType.Deathmatch,
                PlayerCount = Math.Min(_spawnPoints.Count, 4),
                MapName = _currentMapName
            };

            _stateManager.ChangeState(new GameplayState(_game, _contentManager, _inputManager, _audioManager, _stateManager, settings));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Clear with sky blue
            _game.GraphicsDevice.Clear(Color.SkyBlue);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, _camera.GetTransform());

            // Draw tiles
            DrawTiles(spriteBatch);

            // Draw grid
            if (_isGridVisible)
                DrawGrid(spriteBatch);

            // Draw spawn points
            DrawSpawnPoints(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            // Draw UI
            DrawUI(spriteBatch);

            // Draw help overlay
            if (_showHelp)
                DrawHelp(spriteBatch);
        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                for (int y = 0; y < MAP_HEIGHT; y++)
                {
                    var tileType = _mapTiles[x, y];
                    if (tileType != TileType.Empty)
                    {
                        var position = new Vector2(x * TILE_SIZE, y * TILE_SIZE);
                        var sourceRect = _tileSourceRects[tileType];
                        spriteBatch.Draw(_tileTextures, position, sourceRect, Color.White);
                    }
                }
            }
        }

        private void DrawGrid(SpriteBatch spriteBatch)
        {
            var gridColor = new Color(255, 255, 255, 64);

            // Vertical lines
            for (int x = 0; x <= MAP_WIDTH; x++)
            {
                var rect = new Rectangle(x * TILE_SIZE, 0, 1, MAP_HEIGHT * TILE_SIZE);
                spriteBatch.Draw(_gridTexture, rect, gridColor);
            }

            // Horizontal lines
            for (int y = 0; y <= MAP_HEIGHT; y++)
            {
                var rect = new Rectangle(0, y * TILE_SIZE, MAP_WIDTH * TILE_SIZE, 1);
                spriteBatch.Draw(_gridTexture, rect, gridColor);
            }
        }

        private void DrawSpawnPoints(SpriteBatch spriteBatch)
        {
            var playerColors = new[] { Color.Red, Color.Blue, Color.Green, Color.Yellow };

            foreach (var spawn in _spawnPoints)
            {
                var color = playerColors[spawn.PlayerIndex % 4];
                var rect = new Rectangle((int)spawn.Position.X, (int)spawn.Position.Y, TILE_SIZE, TILE_SIZE);
                spriteBatch.Draw(_gridTexture, rect, color * 0.7f);

                // Draw border
                spriteBatch.Draw(_gridTexture, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
                spriteBatch.Draw(_gridTexture, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), color);
                spriteBatch.Draw(_gridTexture, new Rectangle(rect.X, rect.Y, 2, rect.Height), color);
                spriteBatch.Draw(_gridTexture, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), color);
            }
        }

        private void DrawUI(SpriteBatch spriteBatch)
        {
            // Top toolbar
            var toolbarRect = new Rectangle(0, 0, 1280, 60);
            var toolbarBg = new Texture2D(_game.GraphicsDevice, 1, 1);
            toolbarBg.SetData(new[] { new Color(0, 0, 0, 200) });
            spriteBatch.Draw(toolbarBg, toolbarRect, Color.White);

            // Draw mode info
            spriteBatch.DrawString(_font, $"Mode: {_currentMode} ({(int)_currentMode + 1})", new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(_font, $"Tile: {_selectedTile}", new Vector2(10, 35), Color.White);

            // Draw selected tile preview
            if (_selectedTile != TileType.Empty)
            {
                var previewRect = new Rectangle(200, 15, 30, 30);
                spriteBatch.Draw(_tileTextures, previewRect, _tileSourceRects[_selectedTile], Color.White);
            }

            // Draw instructions
            var instructions = "1-4: Tools | Q/E: Tiles | G: Grid | H: Help | F5: Test | Ctrl+S: Save | ESC: Exit";
            var instrSize = _smallFont.MeasureString(instructions);
            spriteBatch.DrawString(_smallFont, instructions, new Vector2(1280 - instrSize.X - 10, 5), Color.LightGray);

            // Draw map info
            spriteBatch.DrawString(_smallFont, $"Map: {_currentMapName} ({MAP_WIDTH}x{MAP_HEIGHT})", new Vector2(1280 - instrSize.X - 10, 30), Color.LightGray);
        }

        private void DrawHelp(SpriteBatch spriteBatch)
        {
            var helpBg = new Texture2D(_game.GraphicsDevice, 1, 1);
            helpBg.SetData(new[] { new Color(0, 0, 0, 220) });
            spriteBatch.Draw(helpBg, new Rectangle(200, 100, 880, 500), Color.White);

            var helpText = new[]
            {
                "LEVEL EDITOR HELP",
                "",
                "TOOLS:",
                "1 - Paint Tool: Place selected tiles",
                "2 - Erase Tool: Remove tiles",
                "3 - Spawn Tool: Place player spawn points",
                "4 - Select Tool: Select and move objects",
                "",
                "CONTROLS:",
                "Mouse Left Click: Use current tool",
                "Mouse Right Click: Sample tile under cursor",
                "WASD / Arrow Keys: Move camera",
                "Shift: Move camera faster",
                "Q/E: Cycle through available tiles",
                "G: Toggle grid visibility",
                "",
                "FILE OPERATIONS:",
                "Ctrl+S: Save current map",
                "Ctrl+O: Load map (placeholder)",
                "Ctrl+N: Create new map",
                "F5: Test map in game",
                "",
                "Press H to close this help"
            };

            for (int i = 0; i < helpText.Length; i++)
            {
                var color = i == 0 ? Color.Yellow : Color.White;
                var font = i == 0 ? _font : _smallFont;
                spriteBatch.DrawString(font, helpText[i], new Vector2(220, 120 + i * 20), color);
            }
        }

        public void Cleanup()
        {
            _tileTextures?.Dispose();
            _gridTexture?.Dispose();
        }
    }

    // Supporting classes for Level Editor
    public enum EditorMode
    {
        Paint = 0,
        Erase = 1,
        Spawn = 2,
        Select = 3
    }

    public class SpawnPoint
    {
        public Vector2 Position { get; set; }
        public int PlayerIndex { get; set; }
        public SpawnType Type { get; set; } = SpawnType.Player;
    }

    public enum SpawnType
    {
        Player,
        PowerUp,
        Enemy,
        Item
    }
}
