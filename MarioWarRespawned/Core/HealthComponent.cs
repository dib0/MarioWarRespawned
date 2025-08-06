namespace MarioWarRespawned.Core
{
    public class HealthComponent : IComponent
    {
        public Entity Owner { get; set; }
        public int MaxHealth { get; set; } = 1;
        public int CurrentHealth { get; set; } = 1;
        public bool IsInvincible { get; set; }
        public float InvincibilityTimer { get; set; }
        public event Action<Entity> OnDeath;

        public void TakeDamage(int damage, Entity attacker = null)
        {
            if (IsInvincible) return;

            CurrentHealth = Math.Max(0, CurrentHealth - damage);
            if (CurrentHealth <= 0)
            {
                OnDeath?.Invoke(attacker);
            }
        }
    }
}
