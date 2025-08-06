using MarioWarRespawned.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MarioWarRespawned.Management;

namespace MarioWarRespawned.GameStates
{
    //SplashState → MainMenuState → GameSetupState → LoadingState → GameplayState
    //                    ↓                ↓                              ↓
    //               OptionsState LevelEditorState              PauseState
    //                    ↓                ↓                              ↓
    //               ControlsState(Test Map) ────────────────► GameOverState
    //               GraphicsState
    //               CreditsState
    //               NetworkLobbyState

    public class GameStateManager
    {
        private readonly Stack<IGameState> _states = new();
        private readonly Game _game;
        private readonly Management.ContentManager _contentManager;
        private readonly InputManager _inputManager;
        private readonly AudioManager _audioManager;

        public GameStateManager(Game game, Management.ContentManager contentManager, InputManager inputManager, AudioManager audioManager)
        {
            _game = game;
            _contentManager = contentManager;
            _inputManager = inputManager;
            _audioManager = audioManager;
        }

        public void Initialize()
        {
            // Start with main menu
            PushState(new MainMenuState(_game, _contentManager, _inputManager, _audioManager, this));
        }

        public void PushState(IGameState state)
        {
            _states.Push(state);
            state.Initialize();
        }

        public void PopState()
        {
            if (_states.Count > 0)
            {
                var state = _states.Pop();
                state.Cleanup();
            }
        }

        public void ChangeState(IGameState newState)
        {
            PopState();
            PushState(newState);
        }

        public void Update(GameTime gameTime)
        {
            if (_states.Count > 0)
            {
                _states.Peek().Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_states.Count > 0)
            {
                _states.Peek().Draw(spriteBatch);
            }
        }
    }

    public interface IGameState
    {
        void Initialize();
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
        void Cleanup();
    }
}
