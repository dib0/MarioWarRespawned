using MarioWarRespawned.Entities;
using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace MarioWarRespawned.GameStates
{
    public class GameOverState : IGameState
    {
        private readonly Game _game;
        private readonly ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;
        private readonly GameStateManager _stateManager;
        private readonly List<Player> _winners;

        private SpriteFont _titleFont;
        private SpriteFont _font;
        private float _animationTimer;
        private int _selectedOption = 0;
        private readonly List<string> _options = new() { "Play Again", "Main Menu", "Exit" };

        public GameOverState(Game game, ContentManager contentManager, InputManager inputManager,
                           AudioManager audioManager, GameStateManager stateManager, List<Player> winners)
        {
            _game = game;
            _contentManager = contentManager;
            _inputManager = inputManager;
            _audioManager = audioManager;
            _stateManager = stateManager;
            _winners = winners ?? new List<Player>();
        }

        public void Initialize()
        {
            _titleFont = _contentManager.GetFont("Title");
            _font = _contentManager.GetFont("Menu");
            _audioManager.StopMusic();

            // Play victory or defeat sound based on results
            if (_winners.Count == 1)
                _audioManager.PlaySound("victory");
            else
                _audioManager.PlaySound("game_over");
        }

        public void Update(GameTime gameTime)
        {
            _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

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
                _audioManager.PlaySound("menu_select");
                HandleSelection();
            }
        }

        private void HandleSelection()
        {
            switch (_selectedOption)
            {
                case 0: // Play Again
                    _stateManager.ChangeState(new GameSetupState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;
                case 1: // Main Menu
                    _stateManager.ChangeState(new MainMenuState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;
                case 2: // Exit
                    _game.Exit();
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw background
            spriteBatch.Draw(
                _contentManager.GetTexture("menu_background") ?? CreateSolidTexture(Color.DarkBlue),
                Vector2.Zero,
                Color.White
            );

            // Draw results
            string resultText;
            Color resultColor;

            if (_winners.Count == 0)
            {
                resultText = "DRAW!";
                resultColor = Color.Yellow;
            }
            else if (_winners.Count == 1)
            {
                resultText = $"{_winners[0].PlayerName.ToUpper()} WINS!";
                resultColor = Color.Gold;
            }
            else
            {
                resultText = "TIE!";
                resultColor = Color.Orange;
            }

            var titleSize = _titleFont.MeasureString(resultText);
            var titlePos = new Vector2(640 - titleSize.X / 2, 100 + MathF.Sin(_animationTimer * 3) * 10);
            spriteBatch.DrawString(_titleFont, resultText, titlePos, resultColor);

            // Draw final scores (assuming we have access to all players)
            var yOffset = 250;
            spriteBatch.DrawString(_font, "FINAL SCORES:", new Vector2(540, yOffset), Color.White);
            yOffset += 50;

            foreach (var winner in _winners.Take(4))
            {
                var scoreText = $"{winner.PlayerName}: {winner.Score} points";
                spriteBatch.DrawString(_font, scoreText, new Vector2(500, yOffset), GetPlayerColor(winner.PlayerId));
                yOffset += 30;
            }

            // Draw menu options
            yOffset = 450;
            for (int i = 0; i < _options.Count; i++)
            {
                var color = i == _selectedOption ? Color.Yellow : Color.White;
                var position = new Vector2(640 - _font.MeasureString(_options[i]).X / 2, yOffset + i * 60);

                if (i == _selectedOption)
                {
                    spriteBatch.DrawString(_font, "> ", position - new Vector2(50, 0), Color.Red);
                }

                spriteBatch.DrawString(_font, _options[i], position, color);
            }
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

        private Texture2D CreateSolidTexture(Color color)
        {
            var texture = new Texture2D(_game.GraphicsDevice, 1, 1);
            texture.SetData(new[] { color });
            return texture;
        }

        public void Cleanup() { }
    }
}
