using MarioWarRespawned.Configuration;
using MarioWarRespawned.Core;
using MarioWarRespawned.Entities;
using MarioWarRespawned.GameModes;
using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using MarioWarRespawned.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MarioWarRespawned.GameStates
{
    public class GameplayState : IGameState
    {
        private readonly Game _game;
        private readonly Management.ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;
        private readonly GameStateManager _stateManager;

        private List<Player> _players = new();
        private List<Entity> _entities = new();
        private GameMode _currentGameMode;
        private Camera2D _camera;
        private SpriteFont _font;
        private float _gameTimer;

        public GameplayState(Game game, Management.ContentManager contentManager, InputManager inputManager,
                           AudioManager audioManager, GameStateManager stateManager, GameSettings settings)
        {
            _game = game;
            _contentManager = contentManager;
            _inputManager = inputManager;
            _audioManager = audioManager;
            _stateManager = stateManager;

            InitializeGameMode(settings);
            InitializePlayers(settings);
        }

        public void Initialize()
        {
            _camera = new Camera2D();
            _font = _contentManager.GetFont("UI");

            // Load player sprites and animations
            foreach (var player in _players)
            {
                var sprite = player.GetComponent<SpriteComponent>();
                sprite.Texture = _contentManager.GetTexture($"mario_player{player.PlayerId}");

                // Setup animations
                var animation = player.GetComponent<AnimationComponent>();
                SetupPlayerAnimations(animation);
            }
        }

        private void InitializeGameMode(GameSettings settings)
        {
            _currentGameMode = settings.GameMode switch
            {
                GameModeType.Deathmatch => new DeathmatchMode(settings),
                GameModeType.CaptureTheFlag => new CaptureTheFlagMode(settings),
                GameModeType.KingOfTheHill => new KingOfTheHillMode(settings),
                _ => new DeathmatchMode(settings)
            };
        }

        private void InitializePlayers(GameSettings settings)
        {
            var spawnPositions = new Vector2[]
            {
                new Vector2(100, 400),
                new Vector2(200, 400),
                new Vector2(1100, 400),
                new Vector2(1000, 400)
            };

            for (int i = 0; i < settings.PlayerCount; i++)
            {
                var player = new Player(i, settings.PlayerNames[i], spawnPositions[i]);
                _players.Add(player);
                _entities.Add(player);
            }

            _currentGameMode.Initialize(_players);
        }

        private void SetupPlayerAnimations(AnimationComponent animation)
        {
            animation.Animations["idle"] = new Animation
            {
                Frames = new[] { new Rectangle(0, 0, 32, 32) },
                FrameDuration = 1.0f,
                IsLooping = true
            };

            animation.Animations["walk"] = new Animation
            {
                Frames = new[]
                {
                    new Rectangle(32, 0, 32, 32),
                    new Rectangle(64, 0, 32, 32),
                    new Rectangle(96, 0, 32, 32)
                },
                FrameDuration = 0.15f,
                IsLooping = true
            };

            animation.Animations["jump"] = new Animation
            {
                Frames = new[] { new Rectangle(128, 0, 32, 32) },
                FrameDuration = 1.0f,
                IsLooping = false
            };

            animation.Animations["fall"] = new Animation
            {
                Frames = new[] { new Rectangle(160, 0, 32, 32) },
                FrameDuration = 1.0f,
                IsLooping = false
            };
        }

        public void Update(GameTime gameTime)
        {
            _gameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update player inputs
            for (int i = 0; i < _players.Count; i++)
            {
                var input = _players[i].GetComponent<InputComponent>();
                input.PreviousInput = input.CurrentInput;
                input.CurrentInput = _inputManager.GetPlayerInput(i);

                // Check for pause
                if (input.CurrentInput.Start && !input.PreviousInput.Start)
                {
                    _stateManager.PushState(new PauseState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    return;
                }
            }

            // Update all entities
            foreach (var entity in _entities.Where(e => e.IsActive))
            {
                entity.Update(gameTime);
            }

            // Update game mode
            _currentGameMode.Update(gameTime);

            // Check win conditions
            if (_currentGameMode.CheckWinCondition(out var winners))
            {
                _stateManager.ChangeState(new GameOverState(_game, _contentManager, _inputManager, _audioManager, _stateManager, winners));
                return;
            }

            // Update camera to follow players
            UpdateCamera();

            // Clean up inactive entities
            _entities.RemoveAll(e => !e.IsActive);
        }

        private void UpdateCamera()
        {
            // Simple camera that centers on all active players
            var activePlayers = _players.Where(p => !p.IsDead).ToList();
            if (activePlayers.Any())
            {
                var centerX = activePlayers.Average(p => p.Position.X);
                var centerY = activePlayers.Average(p => p.Position.Y);
                _camera.Position = new Vector2(centerX - 640, centerY - 360);

                // Keep camera in bounds
                _camera.Position = Vector2.Clamp(_camera.Position, Vector2.Zero, new Vector2(1280 - 1280, 720 - 720));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Apply camera transform
            var transformMatrix = _camera.GetTransform();

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, transformMatrix);

            // Draw background
            DrawBackground(spriteBatch);

            // Draw all entities
            foreach (var entity in _entities.Where(e => e.IsVisible))
            {
                entity.Draw(spriteBatch);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            // Draw UI
            DrawUI(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            // Simple background color for now
            // In a full implementation, you'd draw tile-based backgrounds here
        }

        private void DrawUI(SpriteBatch spriteBatch)
        {
            // Draw player scores and lives
            for (int i = 0; i < _players.Count; i++)
            {
                var player = _players[i];
                var y = 10 + i * 30;
                var color = GetPlayerColor(i);

                spriteBatch.DrawString(_font, $"P{i + 1}: {player.PlayerName}", new Vector2(10, y), color);
                spriteBatch.DrawString(_font, $"Score: {player.Score}", new Vector2(200, y), color);
                spriteBatch.DrawString(_font, $"Lives: {player.Lives}", new Vector2(300, y), color);
            }

            // Draw game mode info
            var timeRemaining = _currentGameMode.GetTimeRemaining();
            if (timeRemaining.HasValue)
            {
                spriteBatch.DrawString(_font, $"Time: {timeRemaining.Value:mm\\:ss}", new Vector2(1100, 10), Color.White);
            }

            spriteBatch.DrawString(_font, _currentGameMode.GetStatusText(), new Vector2(640 - 100, 10), Color.Yellow);
        }

        private Color GetPlayerColor(int playerIndex)
        {
            return playerIndex switch
            {
                0 => Color.Red,
                1 => Color.Blue,
                2 => Color.Green,
                3 => Color.Yellow,
                _ => Color.White
            };
        }

        public void Cleanup()
        {
            _currentGameMode?.Cleanup();
        }
    }
}
