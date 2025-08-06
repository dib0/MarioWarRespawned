using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarioWarRespawned.Core
{
    public class SpriteComponent : IComponent
    {
        public Entity Owner { get; set; }
        public Texture2D Texture { get; set; }
        public Rectangle SourceRectangle { get; set; }
        public Color Tint { get; set; } = Color.White;
        public float Scale { get; set; } = 1.0f;
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;
        public Vector2 Origin { get; set; }
    }
}
