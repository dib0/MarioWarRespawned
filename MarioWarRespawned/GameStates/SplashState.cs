using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MarioWarRespawned.GameStates
{
    public class SplashState : IGameState
    {
        private readonly Game _game;
        private readonly ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;
        private readonly GameStateManager _stateManager;

        private SpriteFont _font;
        private SpriteFont _smallFont;
        private float _fadeTimer = 0f;
        private float _totalTime = 0f;
        private const float FADE_DURATION = 3.0f;
        private const float HOLD_DURATION = 2.0f;
        private const float TOTAL_DURATION = FADE_DURATION + HOLD_DURATION + FADE_DURATION;

        private readonly string[] _credits =
        {
            "Powered by MonoGame",
            "Original concept by Samuele Poletto & Florian Hufsky",
            "Reimagined in C# for modern platforms"
        };

        public SplashState(Game game, ContentManager contentManager, InputManager inputManager,
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
            _font = _contentManager.GetFont("Title") ?? _contentManager.GetFont("Menu");
            _smallFont = _contentManager.GetFont("Menu");
        }

        public void Update(GameTime gameTime)
        {
            _totalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Skip splash on any input
            if (_inputManager.IsKeyPressed(Keys.Enter) ||
                _inputManager.IsKeyPressed(Keys.Escape) ||
                _inputManager.IsKeyPressed(Keys.Space) ||
                _inputManager.GetPlayerInput(0).ActionPressed)
            {
                GoToMainMenu();
                return;
            }

            // Auto-advance after duration
            if (_totalTime >= TOTAL_DURATION)
            {
                GoToMainMenu();
            }
        }

        private void GoToMainMenu()
        {
            _stateManager.ChangeState(new MainMenuState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _game.GraphicsDevice.Clear(Color.Black);

            // Calculate fade alpha
            float alpha = 1.0f;
            if (_totalTime < FADE_DURATION)
            {
                alpha = _totalTime / FADE_DURATION;
            }
            else if (_totalTime > FADE_DURATION + HOLD_DURATION)
            {
                alpha = 1.0f - ((_totalTime - FADE_DURATION - HOLD_DURATION) / FADE_DURATION);
            }

            alpha = MathHelper.Clamp(alpha, 0f, 1f);

            // Draw main logo/title
            var titleText = "MARIO WAR";
            var subtitleText = "RESPAWNED";

            var titleSize = _font.MeasureString(titleText);
            var subtitleSize = _font.MeasureString(subtitleText);

            var titlePos = new Vector2(640 - titleSize.X / 2, 250);
            var subtitlePos = new Vector2(640 - subtitleSize.X / 2, 250 + titleSize.Y + 10);

            spriteBatch.DrawString(_font, titleText, titlePos, Color.Red * alpha);
            spriteBatch.DrawString(_font, subtitleText, subtitlePos, Color.Yellow * alpha);

            // Draw credits
            for (int i = 0; i < _credits.Length; i++)
            {
                var creditSize = _smallFont.MeasureString(_credits[i]);
                var creditPos = new Vector2(640 - creditSize.X / 2, 450 + i * 30);
                spriteBatch.DrawString(_smallFont, _credits[i], creditPos, Color.White * alpha * 0.7f);
            }

            // Draw "Press any key" hint
            if (_totalTime > 1.0f)
            {
                var hintText = "Press any key to continue";
                var hintSize = _smallFont.MeasureString(hintText);
                var hintPos = new Vector2(640 - hintSize.X / 2, 600);
                var hintAlpha = (MathF.Sin(_totalTime * 3) + 1) / 2 * alpha;
                spriteBatch.DrawString(_smallFont, hintText, hintPos, Color.Gray * hintAlpha);
            }
        }

        public void Cleanup() { }
    }
}
