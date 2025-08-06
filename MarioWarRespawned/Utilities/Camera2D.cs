using Microsoft.Xna.Framework;

namespace MarioWarRespawned.Utilities
{
    public class Camera2D
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public float Zoom { get; set; } = 1.0f;
        public Vector2 Origin { get; set; }

        public Matrix GetTransform()
        {
            return Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                   Matrix.CreateRotationZ(Rotation) *
                   Matrix.CreateScale(Zoom) *
                   Matrix.CreateTranslation(new Vector3(Origin.X, Origin.Y, 0));
        }

        public void Follow(Vector2 target, float smoothness = 0.1f)
        {
            Position = Vector2.Lerp(Position, target, smoothness);
        }
    }
}
