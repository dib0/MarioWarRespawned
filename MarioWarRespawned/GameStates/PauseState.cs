using MarioWarRespawned.Configuration;
using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MarioWarRespawned.GameStates
{
    public class PauseState : IGameState
    {
        private readonly Game _game;
        private readonly ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;
        private readonly GameStateManager _stateManager;

        private SpriteFont _titleFont;
        private SpriteFont _menuFont;
        private int _selectedOption = 0;
        private readonly List<string> _options = new()
        {
            "Resume Game",
            "Restart Round",
            "Options",
            "Controls",
            "Main Menu",
            "Exit Game"
        };
        private Texture2D _overlayTexture;
        private float _pulseTimer = 0f;
        private KeyboardState _previousKeyboardState;

        public PauseState(Game game, ContentManager contentManager, InputManager inputManager,
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
            _titleFont = _contentManager.GetFont("Title") ?? _contentManager.GetFont("Menu");
            _menuFont = _contentManager.GetFont("Menu");

            // Create semi-transparent overlay
            _overlayTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
            _overlayTexture.SetData(new[] { new Color(0, 0, 0, 180) });

            _audioManager.PauseMusic();
            _audioManager.PlaySound("menu_select");

            _previousKeyboardState = Keyboard.GetState();
        }

        public void Update(GameTime gameTime)
        {
            _pulseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            var currentKeyboard = Keyboard.GetState();
            var input = _inputManager.GetPlayerInput(0);

            // Menu navigation
            bool upPressed = (currentKeyboard.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.Up)) ||
                           (currentKeyboard.IsKeyDown(Keys.W) && !_previousKeyboardState.IsKeyDown(Keys.W));
            bool downPressed = (currentKeyboard.IsKeyDown(Keys.Down) && !_previousKeyboardState.IsKeyDown(Keys.Down)) ||
                             (currentKeyboard.IsKeyDown(Keys.S) && !_previousKeyboardState.IsKeyDown(Keys.S));

            if (upPressed)
            {
                _selectedOption = (_selectedOption - 1 + _options.Count) % _options.Count;
                _audioManager.PlaySound("menu_move");
            }
            else if (downPressed)
            {
                _selectedOption = (_selectedOption + 1) % _options.Count;
                _audioManager.PlaySound("menu_move");
            }

            // Selection
            bool enterPressed = (currentKeyboard.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter)) ||
                              (currentKeyboard.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space));

            if (input.ActionPressed || enterPressed)
            {
                _audioManager.PlaySound("menu_select");
                HandleSelection();
            }

            // Quick resume with pause key or escape
            bool escapePressed = (currentKeyboard.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape));
            if (input.Start || escapePressed)
            {
                ResumeGame();
            }

            _previousKeyboardState = currentKeyboard;
        }

        private void HandleSelection()
        {
            switch (_selectedOption)
            {
                case 0: // Resume Game
                    ResumeGame();
                    break;

                case 1: // Restart Round
                    _audioManager.StopMusic();
                    // Pop pause state and replace gameplay with new instance
                    _stateManager.PopState();
                    // Would need to pass game settings to restart - simplified for now
                    var settings = new GameSettings(); // You'd want to preserve current settings
                    _stateManager.ChangeState(new GameplayState(_game, _contentManager, _inputManager, _audioManager, _stateManager, settings));
                    break;

                case 2: // Options
                    _stateManager.PushState(new OptionsState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;

                case 3: // Controls
                    _stateManager.PushState(new ControlsState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;

                case 4: // Main Menu
                    _audioManager.StopMusic();
                    _stateManager.ChangeState(new MainMenuState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;

                case 5: // Exit Game
                    _game.Exit();
                    break;
            }
        }

        private void ResumeGame()
        {
            _audioManager.ResumeMusic();
            _audioManager.PlaySound("menu_move");
            _stateManager.PopState();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw semi-transparent overlay over the game
            spriteBatch.Draw(_overlayTexture, new Rectangle(0, 0, 1280, 720), Color.White);

            // Draw "PAUSED" title with pulsing effect
            var pauseText = "PAUSED";
            var pauseSize = _titleFont?.MeasureString(pauseText) ?? _menuFont.MeasureString(pauseText);
            var pausePosition = new Vector2(640 - pauseSize.X / 2, 150);
            var pauseColor = Color.Lerp(Color.Yellow, Color.Red, (MathF.Sin(_pulseTimer * 4) + 1) / 2);

            if (_titleFont != null)
                spriteBatch.DrawString(_titleFont, pauseText, pausePosition, pauseColor);
            else
                spriteBatch.DrawString(_menuFont, pauseText, pausePosition, pauseColor);

            // Draw menu background
            var menuRect = new Rectangle(340, 250, 600, 350);
            var menuBg = new Texture2D(_game.GraphicsDevice, 1, 1);
            menuBg.SetData(new[] { new Color(0, 0, 0, 200) });
            spriteBatch.Draw(menuBg, menuRect, Color.White);

            // Draw border
            var borderColor = Color.Yellow;
            spriteBatch.Draw(menuBg, new Rectangle(menuRect.X - 2, menuRect.Y - 2, menuRect.Width + 4, 2), borderColor);
            spriteBatch.Draw(menuBg, new Rectangle(menuRect.X - 2, menuRect.Bottom, menuRect.Width + 4, 2), borderColor);
            spriteBatch.Draw(menuBg, new Rectangle(menuRect.X - 2, menuRect.Y, 2, menuRect.Height), borderColor);
            spriteBatch.Draw(menuBg, new Rectangle(menuRect.Right, menuRect.Y, 2, menuRect.Height), borderColor);

            // Draw menu options
            for (int i = 0; i < _options.Count; i++)
            {
                var position = new Vector2(400, 280 + i * 45);
                var color = i == _selectedOption ? Color.Yellow : Color.White;
                var scale = i == _selectedOption ? 1.1f : 1.0f;

                // Draw selection indicator
                if (i == _selectedOption)
                {
                    var arrow = "► ";
                    var arrowOffset = MathF.Sin(_pulseTimer * 8) * 5;
                    spriteBatch.DrawString(_menuFont, arrow, position + new Vector2(-40 + arrowOffset, 0), Color.Red, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }

                spriteBatch.DrawString(_menuFont, _options[i], position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            // Draw instructions at bottom
            var instructions = new[]
            {
                "↑↓ Navigate    ENTER Select    ESC Resume",
                "Press ESC or START to resume game quickly"
            };

            for (int i = 0; i < instructions.Length; i++)
            {
                var instrSize = _menuFont.MeasureString(instructions[i]);
                var instrPos = new Vector2(640 - instrSize.X / 2, 650 + i * 25);
                spriteBatch.DrawString(_menuFont, instructions[i], instrPos, Color.Gray);
            }
        }

        public void Cleanup()
        {
            _overlayTexture?.Dispose();
        }
    }
}
