using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarioWarRespawned.GameStates
{
    public class GraphicsState : IGameState
    {
        private readonly Game _game;
        private readonly ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;
        private readonly GameStateManager _stateManager;

        public GraphicsState(Game game, ContentManager contentManager, InputManager inputManager,
                           AudioManager audioManager, GameStateManager stateManager)
        {
            _game = game;
            _contentManager = contentManager;
            _inputManager = inputManager;
            _audioManager = audioManager;
            _stateManager = stateManager;
        }

        public void Initialize() { }

        public void Update(GameTime gameTime)
        {
            if (_inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                _stateManager.PopState();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var font = _contentManager.GetFont("Menu");
            spriteBatch.DrawString(font, "GRAPHICS OPTIONS", new Vector2(100, 100), Color.Yellow);
            spriteBatch.DrawString(font, "Resolution: 1280x720 (Fixed)", new Vector2(100, 200), Color.White);
            spriteBatch.DrawString(font, "Fullscreen: F11", new Vector2(100, 230), Color.White);
            spriteBatch.DrawString(font, "Press ESC to return", new Vector2(100, 600), Color.Gray);
        }

        public void Cleanup() { }
    }
}
