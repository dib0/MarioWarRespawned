using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarioWarRespawned.GameStates
{
    public class LoadingState : IGameState
    {
        private readonly Game _game;
        private readonly ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;
        private readonly GameStateManager _stateManager;
        private readonly IGameState _nextState;

        private SpriteFont _font;
        private float _progress = 0f;
        private float _animationTimer = 0f;
        private string _loadingText = "Loading";
        private readonly string[] _loadingMessages =
        {
            "Loading Mario sprites...",
            "Preparing power-ups...",
            "Setting up game modes...",
            "Loading sound effects...",
            "Initializing physics...",
            "Almost ready..."
        };
        private int _currentMessageIndex = 0;
        private float _messageTimer = 0f;

        public LoadingState(Game game, ContentManager contentManager, InputManager inputManager,
                          AudioManager audioManager, GameStateManager stateManager, IGameState nextState)
        {
            _game = game;
            _contentManager = contentManager;
            _inputManager = inputManager;
            _audioManager = audioManager;
            _stateManager = stateManager;
            _nextState = nextState;
        }

        public void Initialize()
        {
            _font = _contentManager.GetFont("Menu");
        }

        public void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _animationTimer += deltaTime;
            _messageTimer += deltaTime;

            // Update progress (simulate loading)
            _progress += deltaTime * 0.5f; // 2 second loading time

            // Update loading message
            if (_messageTimer > 0.3f && _currentMessageIndex < _loadingMessages.Length - 1)
            {
                _currentMessageIndex++;
                _messageTimer = 0f;
            }

            // Complete loading
            if (_progress >= 1.0f)
            {
                _stateManager.ChangeState(_nextState);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _game.GraphicsDevice.Clear(Color.Black);

            // Draw title
            var title = "MARIO WAR RESPAWNED";
            var titleSize = _font.MeasureString(title);
            var titlePos = new Vector2(640 - titleSize.X / 2, 200);
            spriteBatch.DrawString(_font, title, titlePos, Color.Yellow);

            // Draw loading message
            var message = _loadingMessages[_currentMessageIndex];
            var messageSize = _font.MeasureString(message);
            var messagePos = new Vector2(640 - messageSize.X / 2, 350);
            spriteBatch.DrawString(_font, message, messagePos, Color.White);

            // Draw progress bar
            var barRect = new Rectangle(340, 450, 600, 30);
            var progressRect = new Rectangle(342, 452, (int)((barRect.Width - 4) * _progress), 26);

            var bgTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
            bgTexture.SetData(new[] { Color.DarkGray });
            var progressTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
            progressTexture.SetData(new[] { Color.Green });

            spriteBatch.Draw(bgTexture, barRect, Color.White);
            spriteBatch.Draw(progressTexture, progressRect, Color.White);

            // Draw percentage
            var percentage = $"{_progress * 100:F0}%";
            var percentSize = _font.MeasureString(percentage);
            spriteBatch.DrawString(_font, percentage, new Vector2(640 - percentSize.X / 2, 500), Color.White);

            // Draw animated loading dots
            var dots = "";
            var dotCount = ((int)(_animationTimer * 2) % 4);
            for (int i = 0; i < dotCount; i++) dots += ".";

            var loadingWithDots = _loadingText + dots;
            var loadingSize = _font.MeasureString(loadingWithDots);
            spriteBatch.DrawString(_font, loadingWithDots, new Vector2(640 - loadingSize.X / 2, 550), Color.LightGray);
        }

        public void Cleanup() { }
    }
}
