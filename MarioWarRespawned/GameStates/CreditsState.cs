using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace MarioWarRespawned.GameStates
{
    public class CreditsState : IGameState
    {
        private readonly Game _game;
        private readonly ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;
        private readonly GameStateManager _stateManager;

        private SpriteFont _titleFont;
        private SpriteFont _font;
        private float _scrollOffset = 0;
        private readonly List<string> _creditLines = new()
        {
            "MARIO WAR RESPAWNED",
            "",
            "A Modern C# Reimplementation by:",
            "Division By Zero",
            "",
            "Original Super Mario War by:",
            "Samuele Poletto & Florian Hufsky",
            "",
            "Respawned Version:",
            "Created with MonoGame Framework",
            "",
            "Special Thanks:",
            "Nintendo - For the Mario universe",
            "The MonoGame Community",
            "Original SMW Contributors",
            "",
            "Built with:",
            "MonoGame Framework",
            "C# .NET",
            "Love and Nostalgia",
            "",
            "",
            "Press ESC to return"
        };

        public CreditsState(Game game, ContentManager contentManager, InputManager inputManager,
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
            _titleFont = _contentManager.GetFont("Title");
            _font = _contentManager.GetFont("Menu");
            _scrollOffset = 720; // Start from bottom of screen
        }

        public void Update(GameTime gameTime)
        {
            // Auto-scroll credits
            _scrollOffset -= 50 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Reset scroll when it goes too far up
            if (_scrollOffset < -_creditLines.Count * 40 - 200)
            {
                _scrollOffset = 720;
            }

            // Allow manual exit
            if (_inputManager.IsKeyPressed(Keys.Escape) || _inputManager.GetPlayerInput(0).ActionPressed)
            {
                _stateManager.PopState();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw scrolling credits
            for (int i = 0; i < _creditLines.Count; i++)
            {
                var line = _creditLines[i];
                var yPosition = _scrollOffset + i * 40;

                // Only draw if on screen
                if (yPosition > -50 && yPosition < 770)
                {
                    var font = (i == 0) ? _titleFont : _font;
                    var size = font.MeasureString(line);
                    var xPosition = 640 - size.X / 2;
                    var color = (i == 0) ? Color.Yellow : Color.White;

                    // Add some fade effect at edges
                    if (yPosition < 100) color *= yPosition / 100f;
                    if (yPosition > 620) color *= (720 - yPosition) / 100f;

                    spriteBatch.DrawString(font, line, new Vector2(xPosition, yPosition), color);
                }
            }
        }

        public void Cleanup() { }
    }
}
