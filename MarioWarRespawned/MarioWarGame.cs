using MarioWarRespawned.GameStates;
using MarioWarRespawned.Input;
using MarioWarRespawned.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MarioWarRespawned
{
    public class MarioWarGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameStateManager _gameStateManager;
        private ContentManager _contentManager;
        private InputManager _inputManager;
        private AudioManager _audioManager;

        // Game settings
        public static int ScreenWidth = 1280;
        public static int ScreenHeight = 720;
        public static bool IsFullscreen = false;

        public MarioWarGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Set target resolution
            _graphics.PreferredBackBufferWidth = ScreenWidth;
            _graphics.PreferredBackBufferHeight = ScreenHeight;
            _graphics.IsFullScreen = IsFullscreen;

            // Enable fixed timestep for consistent gameplay
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60); // 60 FPS
        }

        protected override void Initialize()
        {
            // Initialize managers in correct order (but don't load content yet)
            _inputManager = new InputManager();
            _audioManager = new AudioManager();
            _contentManager = new ContentManager(Content);

            base.Initialize(); // This calls LoadContent()

            // Initialize state manager AFTER LoadContent
            _gameStateManager = new GameStateManager(this, _contentManager, _inputManager, _audioManager);
            _audioManager.SetContentManager(_contentManager);
            _gameStateManager.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // CRITICAL: Initialize ContentManager with GraphicsDevice FIRST
            _contentManager.Initialize(GraphicsDevice);

            // THEN load all content
            _contentManager.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // Handle global input
            HandleGlobalInput();

            // Update managers
            _inputManager.Update();
            _gameStateManager?.Update(gameTime);

            base.Update(gameTime);
        }

        private void HandleGlobalInput()
        {
            // Handle fullscreen toggle
            if (_inputManager.IsKeyPressed(Keys.F11))
            {
                ToggleFullscreen();
            }

            // Handle alt+F4 (Windows) for graceful exit
            var keyboard = Keyboard.GetState();
            if ((keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt)) &&
                keyboard.IsKeyDown(Keys.F4))
            {
                Exit();
            }
        }

        private void ToggleFullscreen()
        {
            IsFullscreen = !IsFullscreen;
            _graphics.IsFullScreen = IsFullscreen;
            _graphics.ApplyChanges();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                             SamplerState.PointClamp, null, null, null, null);

            _gameStateManager?.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _contentManager?.UnloadContent();
            base.UnloadContent();
        }
    }
}