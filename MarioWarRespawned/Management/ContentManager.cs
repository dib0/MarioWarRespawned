using MarioWarRespawned.GameStates;
using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace MarioWarRespawned.Management
{
    public class ContentManager
    {
        private readonly Microsoft.Xna.Framework.Content.ContentManager _content;
        private GraphicsDevice _graphicsDevice;
        private readonly Dictionary<string, Texture2D> _textures = new();
        private readonly Dictionary<string, SpriteFont> _fonts = new();
        private readonly Dictionary<string, SoundEffect> _sounds = new();
        private readonly Dictionary<string, Song> _music = new();

        // Cached placeholder textures
        private Texture2D _defaultTexture;
        private Texture2D _missingTexture;

        public ContentManager(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            _content = content;
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            CreatePlaceholderTextures();
        }

        private void CreatePlaceholderTextures()
        {
            // Create default white texture
            _defaultTexture = new Texture2D(_graphicsDevice, 1, 1);
            _defaultTexture.SetData(new[] { Microsoft.Xna.Framework.Color.White });

            // Create "missing texture" placeholder (magenta/black checkerboard)
            _missingTexture = new Texture2D(_graphicsDevice, 32, 32);
            var missingColors = new Microsoft.Xna.Framework.Color[32 * 32];
            for (int i = 0; i < missingColors.Length; i++)
            {
                int x = i % 32;
                int y = i / 32;
                bool checker = ((x / 8) + (y / 8)) % 2 == 0;
                missingColors[i] = checker ? Microsoft.Xna.Framework.Color.Magenta : Microsoft.Xna.Framework.Color.Black;
            }
            _missingTexture.SetData(missingColors);
        }

        public void LoadContent()
        {
            if (_graphicsDevice == null)
                throw new InvalidOperationException("ContentManager must be initialized with GraphicsDevice before loading content");

            // Load fonts (these are required for the game to work)
            LoadFonts();

            // Load textures (with fallbacks)
            LoadTextures();

            // Load sounds (optional)
            LoadSounds();

            // Load music (optional)
            LoadMusic();
        }

        private void LoadFonts()
        {
            // Try to load fonts, use default if not available
            _fonts["UI"] = LoadFontSafe("Fonts/UI");
            _fonts["Title"] = LoadFontSafe("Fonts/Title") ?? _fonts["UI"];
            _fonts["Menu"] = LoadFontSafe("Fonts/Menu") ?? _fonts["UI"];

            // If no fonts loaded, create a minimal fallback (this would require more work in a real project)
            if (_fonts["UI"] == null)
            {
                System.Diagnostics.Debug.WriteLine("WARNING: No fonts could be loaded. Text rendering will not work properly.");
                // Need to include at least one .spritefont file
            }
        }

        private void LoadTextures()
        {
            // Player sprites
            _textures["mario_player0"] = LoadTextureSafe("Sprites/mario_red") ?? CreateColoredPlayerTexture(Microsoft.Xna.Framework.Color.Red);
            _textures["mario_player1"] = LoadTextureSafe("Sprites/mario_blue") ?? CreateColoredPlayerTexture(Microsoft.Xna.Framework.Color.Blue);
            _textures["mario_player2"] = LoadTextureSafe("Sprites/mario_green") ?? CreateColoredPlayerTexture(Microsoft.Xna.Framework.Color.Green);
            _textures["mario_player3"] = LoadTextureSafe("Sprites/mario_yellow") ?? CreateColoredPlayerTexture(Microsoft.Xna.Framework.Color.Yellow);

            // UI textures
            _textures["menu_background"] = LoadTextureSafe("Backgrounds/menu") ?? CreateGradientBackground();

            // Tile textures (create procedurally if missing)
            _textures["tiles"] = LoadTextureSafe("Sprites/tiles") ?? CreateTilesheet();

            // Particle and effect textures
            _textures["pixel"] = _defaultTexture;
            _textures["circle"] = CreateCircleTexture(16);
        }

        private void LoadSounds()
        {
            // Menu sounds
            _sounds["menu_move"] = LoadSoundSafe("Audio/menu_move");
            _sounds["menu_select"] = LoadSoundSafe("Audio/menu_select");

            // Game sounds
            _sounds["jump"] = LoadSoundSafe("Audio/jump");
            _sounds["stomp"] = LoadSoundSafe("Audio/stomp");
            _sounds["coin"] = LoadSoundSafe("Audio/coin");
            _sounds["powerup"] = LoadSoundSafe("Audio/powerup");
            _sounds["victory"] = LoadSoundSafe("Audio/victory");
            _sounds["game_over"] = LoadSoundSafe("Audio/game_over");
        }

        private void LoadMusic()
        {
            // Background music
            _music["menu_theme"] = LoadMusicSafe("Music/menu_theme");
            _music["game_theme"] = LoadMusicSafe("Music/game_theme");
            _music["victory_theme"] = LoadMusicSafe("Music/victory_theme");
        }

        // Safe loading methods that return null instead of throwing exceptions
        private Texture2D LoadTextureSafe(string path)
        {
            try
            {
                return _content.Load<Texture2D>(path);
            }
            catch (ContentLoadException)
            {
                System.Diagnostics.Debug.WriteLine($"Could not load texture: {path}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading texture {path}: {ex.Message}");
                return null;
            }
        }

        private SpriteFont LoadFontSafe(string path)
        {
            try
            {
                return _content.Load<SpriteFont>(path);
            }
            catch (ContentLoadException)
            {
                System.Diagnostics.Debug.WriteLine($"Could not load font: {path}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading font {path}: {ex.Message}");
                return null;
            }
        }

        private SoundEffect LoadSoundSafe(string path)
        {
            try
            {
                return _content.Load<SoundEffect>(path);
            }
            catch (ContentLoadException)
            {
                System.Diagnostics.Debug.WriteLine($"Could not load sound: {path}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sound {path}: {ex.Message}");
                return null;
            }
        }

        private Song LoadMusicSafe(string path)
        {
            try
            {
                return _content.Load<Song>(path);
            }
            catch (ContentLoadException)
            {
                System.Diagnostics.Debug.WriteLine($"Could not load music: {path}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading music {path}: {ex.Message}");
                return null;
            }
        }

        // Procedural texture creation methods
        private Texture2D CreateColoredPlayerTexture(Microsoft.Xna.Framework.Color color)
        {
            var texture = new Texture2D(_graphicsDevice, 128, 32); // 4 frames of 32x32
            var colors = new Microsoft.Xna.Framework.Color[128 * 32];

            // Create simple colored rectangles for each animation frame
            for (int frame = 0; frame < 4; frame++)
            {
                for (int y = 8; y < 24; y++) // Body area
                {
                    for (int x = frame * 32 + 8; x < frame * 32 + 24; x++) // Body width
                    {
                        int index = y * 128 + x;
                        if (index >= 0 && index < colors.Length)
                        {
                            colors[index] = color;
                        }
                    }
                }

                // Add simple head
                for (int y = 4; y < 12; y++)
                {
                    for (int x = frame * 32 + 10; x < frame * 32 + 22; x++)
                    {
                        int index = y * 128 + x;
                        if (index >= 0 && index < colors.Length)
                        {
                            colors[index] = Microsoft.Xna.Framework.Color.Lerp(color, Microsoft.Xna.Framework.Color.White, 0.3f);
                        }
                    }
                }
            }

            texture.SetData(colors);
            return texture;
        }

        private Texture2D CreateGradientBackground()
        {
            var texture = new Texture2D(_graphicsDevice, 1280, 720);
            var colors = new Microsoft.Xna.Framework.Color[1280 * 720];

            for (int i = 0; i < colors.Length; i++)
            {
                int y = i / 1280;
                float gradient = (float)y / 720f;
                colors[i] = Microsoft.Xna.Framework.Color.Lerp(
                    Microsoft.Xna.Framework.Color.DarkBlue,
                    Microsoft.Xna.Framework.Color.LightBlue,
                    gradient
                );
            }

            texture.SetData(colors);
            return texture;
        }

        private Texture2D CreateTilesheet()
        {
            const int tileSize = 32;
            const int tilesPerRow = 8;
            const int rows = 2;
            var texture = new Texture2D(_graphicsDevice, tileSize * tilesPerRow, tileSize * rows);
            var colors = new Microsoft.Xna.Framework.Color[tileSize * tilesPerRow * tileSize * rows];

            // Define tile colors based on TileType enum
            var tileColors = new[]
            {
                Microsoft.Xna.Framework.Color.Transparent,  // Empty
                Microsoft.Xna.Framework.Color.Brown,        // Solid
                Microsoft.Xna.Framework.Color.Orange,       // Platform
                Microsoft.Xna.Framework.Color.Yellow,       // Breakable
                Microsoft.Xna.Framework.Color.Gold,         // Coin
                Microsoft.Xna.Framework.Color.Red,          // Hazard
                Microsoft.Xna.Framework.Color.Blue,         // Water
                Microsoft.Xna.Framework.Color.LightBlue,    // Ice
                Microsoft.Xna.Framework.Color.Gray,         // Conveyor
                Microsoft.Xna.Framework.Color.Green,        // Spring
                Microsoft.Xna.Framework.Color.Purple,       // OneWayLeft
                Microsoft.Xna.Framework.Color.Purple,       // OneWayRight
                Microsoft.Xna.Framework.Color.Purple,       // OneWayUp
                Microsoft.Xna.Framework.Color.Purple,       // OneWayDown
                Microsoft.Xna.Framework.Color.Lime,         // Goal
                Microsoft.Xna.Framework.Color.Cyan          // Checkpoint
            };

            for (int tileIndex = 0; tileIndex < Math.Min(tileColors.Length, tilesPerRow * rows); tileIndex++)
            {
                int tileX = tileIndex % tilesPerRow;
                int tileY = tileIndex / tilesPerRow;
                var color = tileColors[tileIndex];

                // Fill tile area
                for (int y = 0; y < tileSize; y++)
                {
                    for (int x = 0; x < tileSize; x++)
                    {
                        int pixelX = tileX * tileSize + x;
                        int pixelY = tileY * tileSize + y;
                        int index = pixelY * (tileSize * tilesPerRow) + pixelX;

                        if (index >= 0 && index < colors.Length)
                        {
                            // Add a simple border for solid tiles
                            bool isBorder = x == 0 || x == tileSize - 1 || y == 0 || y == tileSize - 1;
                            if (isBorder && color != Microsoft.Xna.Framework.Color.Transparent)
                            {
                                colors[index] = Microsoft.Xna.Framework.Color.Lerp(color, Microsoft.Xna.Framework.Color.Black, 0.3f);
                            }
                            else
                            {
                                colors[index] = color;
                            }
                        }
                    }
                }
            }

            texture.SetData(colors);
            return texture;
        }

        private Texture2D CreateCircleTexture(int radius)
        {
            int diameter = radius * 2;
            var texture = new Texture2D(_graphicsDevice, diameter, diameter);
            var colors = new Microsoft.Xna.Framework.Color[diameter * diameter];

            var center = new Microsoft.Xna.Framework.Vector2(radius, radius);

            for (int i = 0; i < colors.Length; i++)
            {
                int x = i % diameter;
                int y = i / diameter;
                var pos = new Microsoft.Xna.Framework.Vector2(x, y);
                float distance = Microsoft.Xna.Framework.Vector2.Distance(pos, center);

                if (distance <= radius)
                {
                    float alpha = 1.0f - (distance / radius);
                    colors[i] = Microsoft.Xna.Framework.Color.White * alpha;
                }
                else
                {
                    colors[i] = Microsoft.Xna.Framework.Color.Transparent;
                }
            }

            texture.SetData(colors);
            return texture;
        }

        // Public accessors
        public Texture2D GetTexture(string name) => _textures.GetValueOrDefault(name) ?? _missingTexture;
        public SpriteFont GetFont(string name) => _fonts.GetValueOrDefault(name);
        public SoundEffect GetSound(string name) => _sounds.GetValueOrDefault(name);
        public Song GetMusic(string name) => _music.GetValueOrDefault(name);

        // Utility methods
        public Texture2D GetDefaultTexture() => _defaultTexture;
        public Texture2D GetMissingTexture() => _missingTexture;

        public Texture2D CreateSolidColorTexture(Microsoft.Xna.Framework.Color color, int width = 1, int height = 1)
        {
            var texture = new Texture2D(_graphicsDevice, width, height);
            var colors = new Microsoft.Xna.Framework.Color[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            texture.SetData(colors);
            return texture;
        }

        public void UnloadContent()
        {
            // Dispose of created textures
            _defaultTexture?.Dispose();
            _missingTexture?.Dispose();

            foreach (var texture in _textures.Values)
            {
                texture?.Dispose();
            }

            _textures.Clear();
            _fonts.Clear();
            _sounds.Clear();
            _music.Clear();
        }
    }
}
