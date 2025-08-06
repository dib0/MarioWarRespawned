using MarioWarRespawned.Configuration;
using MarioWarRespawned.GameStates;
using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MarioWarRespawned.GameStates
{
    public class ControlsState : IGameState
    {
        private readonly Game _game;
        private readonly ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;
        private readonly GameStateManager _stateManager;

        public ControlsState(Game game, ContentManager contentManager, InputManager inputManager,
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
            spriteBatch.DrawString(font, "CONTROLS", new Vector2(100, 100), Color.Yellow);
            spriteBatch.DrawString(font, "Player 1: WASD + Ctrl", new Vector2(100, 200), Color.White);
            spriteBatch.DrawString(font, "Players 2-4: Xbox Controllers", new Vector2(100, 230), Color.White);
            spriteBatch.DrawString(font, "ESC - Pause/Back", new Vector2(100, 300), Color.White);
            spriteBatch.DrawString(font, "Press ESC to return", new Vector2(100, 600), Color.Gray);
        }

        public void Cleanup() { }
    }
}