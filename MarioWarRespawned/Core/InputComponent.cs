using MarioWarRespawned.Input;
using Microsoft.Xna.Framework;

namespace MarioWarRespawned.Core
{
    public class InputComponent : IComponent
    {
        public Entity Owner { get; set; }
        public int PlayerIndex { get; set; }
        public PlayerInput CurrentInput { get; set; } = new PlayerInput();
        public PlayerInput PreviousInput { get; set; } = new PlayerInput();
    }
}
