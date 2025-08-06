using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace MarioWarRespawned.GameStates
{
    public class OptionsState : IGameState
    {
        private readonly Game _game;
        private readonly ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;
        private readonly GameStateManager _stateManager;

        private SpriteFont _font;
        private int _selectedOption = 0;
        private readonly List<string> _options = new()
        {
            "Sound Volume",
            "Music Volume",
            "Controls",
            "Graphics",
            "Back"
        };

        public OptionsState(Game game, ContentManager contentManager, InputManager inputManager,
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
        }

        public void Update(GameTime gameTime)
        {
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

            var input = _inputManager.GetPlayerInput(0);
            if (input.ActionPressed || _inputManager.IsKeyPressed(Keys.Enter))
            {
                HandleSelection();
            }
            else if (_inputManager.IsKeyPressed(Keys.Left) || _inputManager.IsKeyPressed(Keys.Right))
            {
                HandleValueChange(_inputManager.IsKeyPressed(Keys.Right));
            }

            if (_inputManager.IsKeyPressed(Keys.Escape))
            {
                _stateManager.PopState();
            }
        }

        private void HandleSelection()
        {
            switch (_selectedOption)
            {
                case 2: // Controls
                    _stateManager.PushState(new ControlsState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;
                case 3: // Graphics
                    _stateManager.PushState(new GraphicsState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;
                case 4: // Back
                    _stateManager.PopState();
                    break;
            }
        }

        private void HandleValueChange(bool increase)
        {
            const float step = 0.1f;

            switch (_selectedOption)
            {
                case 0: // Sound Volume
                    _audioManager.SoundVolume += increase ? step : -step;
                    _audioManager.PlaySound("menu_move");
                    break;
                case 1: // Music Volume
                    _audioManager.MusicVolume += increase ? step : -step;
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var title = "OPTIONS";
            var titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title, new Vector2(640 - titleSize.X / 2, 50), Color.Yellow);

            for (int i = 0; i < _options.Count; i++)
            {
                var position = new Vector2(200, 150 + i * 60);
                var color = i == _selectedOption ? Color.Yellow : Color.White;

                spriteBatch.DrawString(_font, _options[i], position, color);

                // Draw current values
                string value = i switch
                {
                    0 => $"{_audioManager.SoundVolume:P0}",
                    1 => $"{_audioManager.MusicVolume:P0}",
                    _ => ""
                };

                if (!string.IsNullOrEmpty(value))
                {
                    spriteBatch.DrawString(_font, value, position + new Vector2(300, 0), color);
                }

                if (i == _selectedOption && i < 2)
                {
                    spriteBatch.DrawString(_font, "< >", position + new Vector2(250, 0), Color.Red);
                }
            }

            spriteBatch.DrawString(_font, "Use ARROW KEYS to navigate, ESC to go back", new Vector2(200, 600), Color.Gray);
        }

        public void Cleanup() { }
    }
}
