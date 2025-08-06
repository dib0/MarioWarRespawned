using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MarioWarRespawned.Core;
using System;
using MarioWarRespawned.Security;
using MarioWarRespawned.Input;

namespace MarioWarRespawned.Entities
{
    public class Player : Entity
    {
        public int PlayerId { get; }
        public string PlayerName { get; }
        public PlayerState State { get; set; } = PlayerState.Small;
        public int Lives { get; set; } = 3;
        public int Score { get; set; }
        public PowerUpType CurrentPowerUp { get; set; } = PowerUpType.None;
        public bool IsDead { get; set; }
        public float RespawnTimer { get; set; }
        public Vector2 SpawnPosition { get; set; }

        private const float MOVE_SPEED = 200f;
        private const float JUMP_POWER = 400f;
        private const float RESPAWN_TIME = 3f;

        public Player(int playerId, string playerName, Vector2 spawnPosition)
        {
            PlayerId = playerId;
            PlayerName = SecurityHelper.SanitizePlayerName(playerName);
            SpawnPosition = spawnPosition;
            Position = spawnPosition;

            // Add components
            AddComponent(new SpriteComponent
            {
                SourceRectangle = new Rectangle(0, 0, 32, 32),
                Origin = new Vector2(16, 32)
            });

            AddComponent(new PhysicsComponent
            {
                BoundingBox = new Rectangle(-8, -32, 16, 32),
                MaxSpeed = MOVE_SPEED,
                JumpPower = JUMP_POWER
            });

            AddComponent(new InputComponent { PlayerIndex = playerId });

            var health = new HealthComponent();
            health.OnDeath += OnPlayerDeath;
            AddComponent(health);

            AddComponent(new AnimationComponent());
        }

        public override void Update(GameTime gameTime)
        {
            if (IsDead)
            {
                RespawnTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (RespawnTimer <= 0)
                {
                    Respawn();
                }
                return;
            }

            var input = GetComponent<InputComponent>();
            var physics = GetComponent<PhysicsComponent>();
            var sprite = GetComponent<SpriteComponent>();
            var animation = GetComponent<AnimationComponent>();

            // Process input
            ProcessMovement(input.CurrentInput, physics, gameTime);
            ProcessActions(input.CurrentInput, gameTime);

            // Update physics
            UpdatePhysics(physics, gameTime);

            // Update animations
            UpdateAnimations(animation, physics, gameTime);

            // Update sprite effects
            if (physics.Velocity.X > 0) sprite.Effects = SpriteEffects.None;
            else if (physics.Velocity.X < 0) sprite.Effects = SpriteEffects.FlipHorizontally;
        }

        private void ProcessMovement(PlayerInput input, PhysicsComponent physics, GameTime gameTime)
        {
            // Horizontal movement
            if (input.Left)
            {
                physics.Velocity = new(Math.Max(physics.Velocity.X - MOVE_SPEED * 4 * (float)gameTime.ElapsedGameTime.TotalSeconds, -physics.MaxSpeed), 
                                        physics.Velocity.Y);
            }
            else if (input.Right)
            {
                physics.Velocity = new(Math.Min(physics.Velocity.X + MOVE_SPEED * 4 * (float)gameTime.ElapsedGameTime.TotalSeconds, physics.MaxSpeed),
                                        physics.Velocity.Y);
            }
            else
            {
                // Apply friction
                if (Math.Abs(physics.Velocity.X) < 1f)
                {
                    physics.Velocity = new(0, physics.Velocity.Y);
                }
                else
                {
                    physics.Velocity = new(physics.Velocity.X * physics.Friction,
                                            physics.Velocity.Y);
                }
            }

            // Jumping
            if (input.Jump && physics.OnGround)
            {
                physics.Velocity = new(physics.Velocity.X,
                                        physics.Velocity.Y - physics.JumpPower);
                physics.OnGround = false;
            }
        }

        private void ProcessActions(PlayerInput input, GameTime gameTime)
        {
            if (input.Action)
            {
                // Fire projectile, use power-up, etc.
                UseCurrentPowerUp();
            }
        }

        private void UpdatePhysics(PhysicsComponent physics, GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Apply gravity
            if (physics.AffectedByGravity && !physics.OnGround)
            {
                physics.Velocity = new(physics.Velocity.X,
                                        physics.Velocity.Y + physics.Gravity * deltaTime);
            }

            // Update position
            Position += physics.Velocity * deltaTime;

            // Simple ground collision (would be replaced with proper tile collision)
            if (Position.Y > 400) // Ground level
            {
                Position = new Vector2(Position.X, 400);
                physics.Velocity = new(physics.Velocity.X, 0);
                physics.OnGround = true;
            }
            else
            {
                physics.OnGround = false;
            }

            // Screen boundaries
            Position = new Vector2(
                MathHelper.Clamp(Position.X, 16, 1264),
                Position.Y
            );
        }

        private void UpdateAnimations(AnimationComponent animation, PhysicsComponent physics, GameTime gameTime)
        {
            // Determine animation state
            string targetAnimation = "idle";

            if (!physics.OnGround)
            {
                targetAnimation = physics.Velocity.Y < 0 ? "jump" : "fall";
            }
            else if (Math.Abs(physics.Velocity.X) > 10)
            {
                targetAnimation = "walk";
            }

            animation.PlayAnimation(targetAnimation);

            // Update animation timer
            if (animation.CurrentAnimation != null)
            {
                animation.AnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (animation.AnimationTimer >= animation.CurrentAnimation.FrameDuration)
                {
                    animation.AnimationTimer = 0;
                    animation.CurrentAnimation.CurrentFrame++;

                    if (animation.CurrentAnimation.CurrentFrame >= animation.CurrentAnimation.Frames.Length)
                    {
                        if (animation.CurrentAnimation.IsLooping)
                        {
                            animation.CurrentAnimation.CurrentFrame = 0;
                        }
                        else
                        {
                            animation.CurrentAnimation.CurrentFrame = animation.CurrentAnimation.Frames.Length - 1;
                        }
                    }
                }
            }
        }

        private void UseCurrentPowerUp()
        {
            switch (CurrentPowerUp)
            {
                case PowerUpType.FireFlower:
                    // Create fireball projectile
                    break;
                case PowerUpType.IceFlower:
                    // Create ice projectile
                    break;
                case PowerUpType.Star:
                    // Activate invincibility
                    break;
            }
        }

        private void OnPlayerDeath(Entity killer)
        {
            IsDead = true;
            Lives--;
            RespawnTimer = RESPAWN_TIME;
            IsVisible = false;

            // Award points to killer if it's another player
            if (killer is Player killerPlayer && killerPlayer != this)
            {
                killerPlayer.Score += 100;
            }
        }

        private void Respawn()
        {
            if (Lives > 0)
            {
                IsDead = false;
                IsVisible = true;
                Position = SpawnPosition;
                Velocity = Vector2.Zero;
                State = PlayerState.Small;
                CurrentPowerUp = PowerUpType.None;
                GetComponent<HealthComponent>().CurrentHealth = GetComponent<HealthComponent>().MaxHealth;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            var sprite = GetComponent<SpriteComponent>();
            var animation = GetComponent<AnimationComponent>();

            if (sprite?.Texture != null)
            {
                Rectangle sourceRect = sprite.SourceRectangle;

                // Use animation frame if available
                if (animation?.CurrentAnimation != null)
                {
                    var frame = animation.CurrentAnimation.CurrentFrame;
                    if (frame < animation.CurrentAnimation.Frames.Length)
                    {
                        sourceRect = animation.CurrentAnimation.Frames[frame];
                    }
                }

                spriteBatch.Draw(
                    sprite.Texture,
                    Position,
                    sourceRect,
                    sprite.Tint,
                    0f,
                    sprite.Origin,
                    sprite.Scale,
                    sprite.Effects,
                    Layer
                );
            }
        }
    }

    public enum PlayerState
    {
        Small,
        Super,
        Fire,
        Ice,
        Invincible,
        Dead
    }

    public enum PowerUpType
    {
        None,
        Mushroom,
        FireFlower,
        IceFlower,
        Star,
        OneUp
    }
}
