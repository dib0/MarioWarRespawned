using MarioWarRespawned.Core;
using Microsoft.Xna.Framework;

namespace MarioWarRespawned.Core
{
    // Bit flags for collision layers
    [Flags]
    public enum LayerMask
    {
        None = 0,
        Player = 1,
        Enemy = 2,
        Projectile = 4,
        Collectible = 8,
        Environment = 16,
        Trigger = 32,
        All = ~0
    }

    public class PhysicsComponent : IComponent
    {
        public Entity Owner { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public float Friction { get; set; } = 0.9f;
        public float Gravity { get; set; } = 800f;
        public bool AffectedByGravity { get; set; } = true;
        public Rectangle BoundingBox { get; set; }
        public bool OnGround { get; set; }
        public float MaxSpeed { get; set; } = 300f;
        public float JumpPower { get; set; } = 400f;

        // Additional physics properties for more advanced physics
        public float Mass { get; set; } = 1.0f;
        public float Bounciness { get; set; } = 0.0f;
        public bool IsStatic { get; set; } = false;
        public Vector2 LastPosition { get; set; }

        // Collision properties
        public bool IsSolid { get; set; } = true;
        public bool IsTrigger { get; set; } = false;
        public LayerMask CollisionMask { get; set; } = LayerMask.All;

        // Terminal velocity to prevent infinite falling speeds
        public float TerminalVelocity { get; set; } = 600f;

        public PhysicsComponent()
        {
            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
            LastPosition = Vector2.Zero;
        }

        /// <summary>
        /// Applies a force to this physics body
        /// </summary>
        public void AddForce(Vector2 force)
        {
            if (!IsStatic)
            {
                Acceleration += force / Mass;
            }
        }

        /// <summary>
        /// Applies an impulse (instant velocity change) to this physics body
        /// </summary>
        public void AddImpulse(Vector2 impulse)
        {
            if (!IsStatic)
            {
                Velocity += impulse / Mass;
            }
        }

        /// <summary>
        /// Clamps velocity to maximum speeds
        /// </summary>
        public void ClampVelocity()
        {
            // Clamp horizontal velocity
            if (Math.Abs(Velocity.X) > MaxSpeed)
            {
                Velocity = new Vector2(Math.Sign(Velocity.X) * MaxSpeed, Velocity.Y);
            }

            // Clamp vertical velocity (terminal velocity)
            if (Velocity.Y > TerminalVelocity)
            {
                Velocity = new Vector2(Velocity.X, TerminalVelocity);
            }
        }

        /// <summary>
        /// Gets the world-space bounding box for collision detection
        /// </summary>
        public Rectangle GetWorldBounds(Vector2 entityPosition)
        {
            return new Rectangle(
                (int)(entityPosition.X + BoundingBox.X),
                (int)(entityPosition.Y + BoundingBox.Y),
                BoundingBox.Width,
                BoundingBox.Height
            );
        }
    }
}