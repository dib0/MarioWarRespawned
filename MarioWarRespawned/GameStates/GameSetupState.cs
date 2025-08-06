using MarioWarRespawned.Configuration;
using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace MarioWarRespawned.GameStates
{
    public class GameSetupState : IGameState
    {
        private readonly Game _game;
        private readonly ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;
        private readonly GameStateManager _stateManager;

        private SpriteFont _font;
        private GameSettings _settings;
        private int _selectedOption = 0;
        private readonly List<string> _options = new()
        {
            "Game Mode",
            "Player Count",
            "Kill Limit",
            "Time Limit",
            "Start Game",
            "Back"
        };

        public GameSetupState(Game game, ContentManager contentManager, InputManager inputManager,
                            AudioManager audioManager, GameStateManager stateManager)
        {
            _game = game;
            _contentManager = contentManager;
            _inputManager = inputManager;
            _audioManager = audioManager;
            _stateManager = stateManager;
            _settings = new GameSettings();
        }

        public void Initialize()
        {
            _font = _contentManager.GetFont("Menu");
        }

        public void Update(GameTime gameTime)
        {
            var input = _inputManager.GetPlayerInput(0);

            // Navigation
            if (_inputManager.IsKeyPressed(Keys.Up))
            {
                _selectedOption = (_selectedOption - 1 + _options.Count) % _options.Count;
                _audioManager.PlaySound("menu_move");
            }
            else if (_inputManager.IsKeyPressed(Keys.Down))
            {
                _selectedOption = (_selectedOption + 1) % _options.Count;
                _audioManager.PlaySound("menu_move");
            }

            if (input.ActionPressed || _inputManager.IsKeyPressed(Keys.Enter))
            {
                _audioManager.PlaySound("menu_select");
                HandleSelection();
            }

            // Resume with pause key
            if (input.Start || _inputManager.IsKeyPressed(Keys.Escape))
            {
                ResumeGame();
            }
        }

        private void HandleSelection()
        {
            switch (_selectedOption)
            {
                case 0: // Resume
                    ResumeGame();
                    break;
                case 1: // Options
                    _stateManager.PushState(new OptionsState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;
                case 2: // Main Menu
                    _audioManager.StopMusic();
                    _stateManager.ChangeState(new MainMenuState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;
                case 3: // Exit Game
                    _game.Exit();
                    break;
            }
        }

        private void ResumeGame()
        {
            _audioManager.ResumeMusic();
            _stateManager.PopState();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw semi-transparent overlay
            var overlay = new Texture2D(_game.GraphicsDevice, 1, 1);
            overlay.SetData([new Color(0, 0, 0, 128)]);
            spriteBatch.Draw(overlay, new Rectangle(0, 0, 1280, 720), Color.White);

            // Draw pause menu
            var title = "PAUSED";
            var titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title, new Vector2(640 - titleSize.X / 2, 200), Color.Yellow);

            for (int i = 0; i < _options.Count; i++)
            {
                var color = i == _selectedOption ? Color.Yellow : Color.White;
                var position = new Vector2(640 - _font.MeasureString(_options[i]).X / 2, 300 + i * 60);

                if (i == _selectedOption)
                {
                    spriteBatch.DrawString(_font, "> ", position - new Vector2(50, 0), Color.Red);
                }

                spriteBatch.DrawString(_font, _options[i], position, color);
            }
        }

        public void Cleanup() { }
    }
}
