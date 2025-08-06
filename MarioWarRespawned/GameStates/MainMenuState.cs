using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static System.Net.Mime.MediaTypeNames;

namespace MarioWarRespawned.GameStates
{
    public class MainMenuState : IGameState
    {
        private readonly Game _game;
        private readonly Management.ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;
        private readonly GameStateManager _stateManager;

        private SpriteFont _titleFont;
        private SpriteFont _menuFont;
        private readonly List<string> _menuItems;
        private int _selectedIndex = 0;
        private Texture2D _backgroundTexture;
        private float _animationTimer;

        public MainMenuState(Game game, Management.ContentManager contentManager, InputManager inputManager,
                           AudioManager audioManager, GameStateManager stateManager)
        {
            _game = game;
            _contentManager = contentManager;
            _inputManager = inputManager;
            _audioManager = audioManager;
            _stateManager = stateManager;

            _menuItems =
            [
                "Start Game",
                "Options",
                "Level Editor",
                "Credits",
                "Exit"
            ];
        }

        public void Initialize()
        {
            _titleFont = _contentManager.GetFont("Title");
            _menuFont = _contentManager.GetFont("Menu");
            _backgroundTexture = _contentManager.GetTexture("menu_background");

            _audioManager.PlayMusic("menu_theme");
        }

        public void Update(GameTime gameTime)
        {
            _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            var input = _inputManager.GetPlayerInput(0);

            // Menu navigation
            if (input.JumpPressed || _inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                _selectedIndex = (_selectedIndex - 1 + _menuItems.Count) % _menuItems.Count;
                _audioManager.PlaySound("menu_move");
            }
            else if (_inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                _selectedIndex = (_selectedIndex + 1) % _menuItems.Count;
                _audioManager.PlaySound("menu_move");
            }

            // Menu selection
            if (_inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                _audioManager.PlaySound("menu_select");
                HandleMenuSelection();
            }
        }

        private void HandleMenuSelection()
        {
            switch (_selectedIndex)
            {
                case 0: // Start Game
                    _stateManager.ChangeState(new GameSetupState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;
                case 1: // Options
                    _stateManager.PushState(new OptionsState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;
                case 2: // Level Editor
                    _stateManager.ChangeState(new LevelEditorState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;
                case 3: // Credits
                    _stateManager.PushState(new CreditsState(_game, _contentManager, _inputManager, _audioManager, _stateManager));
                    break;
                case 4: // Exit
                    _game.Exit();
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw background
            if (_backgroundTexture != null)
            {
                spriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);
            }

            // Draw title with animation
            var titleText = "MARIO WAR RESPAWNED";
            var titleSize = _titleFont.MeasureString(titleText);
            var titlePos = new Vector2(640 - titleSize.X / 2, 100 + MathF.Sin(_animationTimer * 2) * 10);
            var titleColor = Color.Lerp(Color.Red, Color.Yellow, (MathF.Sin(_animationTimer * 3) + 1) / 2);

            spriteBatch.DrawString(_titleFont, titleText, titlePos, titleColor);

            // Draw menu items
            for (int i = 0; i < _menuItems.Count; i++)
            {
                var color = i == _selectedIndex ? Color.Yellow : Color.White;
                var position = new Vector2(640 - _menuFont.MeasureString(_menuItems[i]).X / 2, 300 + i * 60);

                if (i == _selectedIndex)
                {
                    // Draw selection indicator
                    spriteBatch.DrawString(_menuFont, "> ", position - new Vector2(50, 0), Color.Red);
                    spriteBatch.DrawString(_menuFont, " <", position + new Vector2(_menuFont.MeasureString(_menuItems[i]).X, 0), Color.Red);
                }

                spriteBatch.DrawString(_menuFont, _menuItems[i], position, color);
            }

            // Draw version info
            spriteBatch.DrawString(_menuFont, "v1.0.0", new Vector2(10, 680), Color.Gray);
        }

        public void Cleanup()
        {
            // Cleanup resources if needed
        }
    }
}
