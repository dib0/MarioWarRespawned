using MarioWarRespawned.Security;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarioWarRespawned.Input
{
    public class InputManager
    {
        private KeyboardState _currentKeyboard;
        private KeyboardState _previousKeyboard;
        private GamePadState[] _currentGamePads;
        private GamePadState[] _previousGamePads;

        public InputManager()
        {
            _currentGamePads = new GamePadState[4];
            _previousGamePads = new GamePadState[4];
        }

        public void Update()
        {
            _previousKeyboard = _currentKeyboard;
            _currentKeyboard = Keyboard.GetState();

            for (int i = 0; i < 4; i++)
            {
                _previousGamePads[i] = _currentGamePads[i];
                _currentGamePads[i] = GamePad.GetState(i);
            }
        }

        public PlayerInput GetPlayerInput(int playerIndex)
        {
            var input = new PlayerInput();

            if (playerIndex == 0) // Player 1 - Keyboard
            {
                input.Left = _currentKeyboard.IsKeyDown(Keys.A) || _currentKeyboard.IsKeyDown(Keys.Left);
                input.Right = _currentKeyboard.IsKeyDown(Keys.D) || _currentKeyboard.IsKeyDown(Keys.Right);
                input.Jump = _currentKeyboard.IsKeyDown(Keys.W) || _currentKeyboard.IsKeyDown(Keys.Up) || _currentKeyboard.IsKeyDown(Keys.Space);
                input.Action = _currentKeyboard.IsKeyDown(Keys.S) || _currentKeyboard.IsKeyDown(Keys.Down) || _currentKeyboard.IsKeyDown(Keys.LeftControl);
                input.Start = _currentKeyboard.IsKeyDown(Keys.Enter) || _currentKeyboard.IsKeyDown(Keys.Escape);

                input.JumpPressed = (_currentKeyboard.IsKeyDown(Keys.W) && !_previousKeyboard.IsKeyDown(Keys.W)) ||
                                  (_currentKeyboard.IsKeyDown(Keys.Up) && !_previousKeyboard.IsKeyDown(Keys.Up)) ||
                                  (_currentKeyboard.IsKeyDown(Keys.Space) && !_previousKeyboard.IsKeyDown(Keys.Space));

                input.ActionPressed = (_currentKeyboard.IsKeyDown(Keys.S) && !_previousKeyboard.IsKeyDown(Keys.S)) ||
                                    (_currentKeyboard.IsKeyDown(Keys.Down) && !_previousKeyboard.IsKeyDown(Keys.Down)) ||
                                    (_currentKeyboard.IsKeyDown(Keys.LeftControl) && !_previousKeyboard.IsKeyDown(Keys.LeftControl));
            }
            else if (playerIndex > 0 && playerIndex < 4) // Gamepad players
            {
                var gamepad = _currentGamePads[playerIndex];
                var prevGamepad = _previousGamePads[playerIndex];

                if (gamepad.IsConnected)
                {
                    input.Left = gamepad.DPad.Left == ButtonState.Pressed || gamepad.ThumbSticks.Left.X < -0.5f;
                    input.Right = gamepad.DPad.Right == ButtonState.Pressed || gamepad.ThumbSticks.Left.X > 0.5f;
                    input.Jump = gamepad.Buttons.A == ButtonState.Pressed || gamepad.DPad.Up == ButtonState.Pressed;
                    input.Action = gamepad.Buttons.X == ButtonState.Pressed || gamepad.Buttons.B == ButtonState.Pressed;
                    input.Start = gamepad.Buttons.Start == ButtonState.Pressed;

                    input.JumpPressed = gamepad.Buttons.A == ButtonState.Pressed && prevGamepad.Buttons.A == ButtonState.Released;
                    input.ActionPressed = gamepad.Buttons.X == ButtonState.Pressed && prevGamepad.Buttons.X == ButtonState.Released;
                }
            }

            return SecurityHelper.ValidateInput(input) ? input : new PlayerInput();
        }

        public bool IsKeyPressed(Keys key)
        {
            return _currentKeyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
        }

        public bool IsButtonPressed(int playerIndex, Buttons button)
        {
            if (playerIndex < 0 || playerIndex >= 4) return false;
            return _currentGamePads[playerIndex].IsButtonDown(button) && !_previousGamePads[playerIndex].IsButtonDown(button);
        }
    }

    public class PlayerInput
    {
        public bool Left { get; set; }
        public bool Right { get; set; }
        public bool Jump { get; set; }
        public bool Action { get; set; }
        public bool Start { get; set; }
        public bool JumpPressed { get; set; }
        public bool ActionPressed { get; set; }
    }
}
