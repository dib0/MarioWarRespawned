using Microsoft.Xna.Framework;
using MarioWarRespawned.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using MarioWarRespawned.Configuration;

namespace MarioWarRespawned.GameModes
{
    public abstract class GameMode
    {
        protected List<Player> Players = new();
        protected GameSettings Settings;
        protected float GameTimer;

        public abstract string Name { get; }
        public abstract string Description { get; }

        protected GameMode(GameSettings settings)
        {
            Settings = settings;
        }

        public virtual void Initialize(List<Player> players)
        {
            Players = players;
            GameTimer = 0;
            OnInitialize();
        }

        protected virtual void OnInitialize() { }

        public virtual void Update(GameTime gameTime)
        {
            GameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            OnUpdate(gameTime);
        }

        protected virtual void OnUpdate(GameTime gameTime) { }

        public abstract bool CheckWinCondition(out List<Player> winners);

        public virtual void OnPlayerDeath(Player player, Player killer)
        {
            if (killer != null && killer != player)
            {
                killer.Score += GetKillPoints();
            }
        }

        protected virtual int GetKillPoints() => 100;

        public virtual TimeSpan? GetTimeRemaining()
        {
            if (Settings.TimeLimit > 0)
            {
                var remaining = Settings.TimeLimit - GameTimer;
                return remaining > 0 ? TimeSpan.FromSeconds(remaining) : TimeSpan.Zero;
            }
            return null;
        }

        public virtual string GetStatusText()
        {
            return Name;
        }

        public virtual void Cleanup() { }
    }

    public class DeathmatchMode : GameMode
    {
        public override string Name => "Deathmatch";
        public override string Description => "First to reach the kill limit wins!";

        private readonly Dictionary<Player, int> _playerKills = new();

        public DeathmatchMode(GameSettings settings) : base(settings) { }

        protected override void OnInitialize()
        {
            foreach (var player in Players)
            {
                _playerKills[player] = 0;
            }
        }

        public override bool CheckWinCondition(out List<Player> winners)
        {
            winners = new List<Player>();

            // Check kill limit
            if (Settings.KillLimit > 0)
            {
                winners = _playerKills
                    .Where(kvp => kvp.Value >= Settings.KillLimit)
                    .Select(kvp => kvp.Key)
                    .ToList();

                if (winners.Any()) return true;
            }

            // Check time limit
            var timeRemaining = GetTimeRemaining();
            if (timeRemaining.HasValue && timeRemaining.Value <= TimeSpan.Zero)
            {
                var maxKills = _playerKills.Values.Max();
                winners = _playerKills
                    .Where(kvp => kvp.Value == maxKills)
                    .Select(kvp => kvp.Key)
                    .ToList();
                return true;
            }

            return false;
        }

        public override void OnPlayerDeath(Player player, Player killer)
        {
            base.OnPlayerDeath(player, killer);

            if (killer != null && killer != player)
            {
                _playerKills[killer] = _playerKills.GetValueOrDefault(killer) + 1;
            }
        }

        public override string GetStatusText()
        {
            if (Settings.KillLimit > 0)
            {
                var leader = _playerKills.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
                return $"First to {Settings.KillLimit} kills! Leader: {leader.Key?.PlayerName} ({leader.Value})";
            }
            return base.GetStatusText();
        }
    }

    public class CaptureTheFlagMode : GameMode
    {
        public override string Name => "Capture The Flag";
        public override string Description => "Capture the enemy flag and return it to your base!";

        public CaptureTheFlagMode(GameSettings settings) : base(settings) { }

        public override bool CheckWinCondition(out List<Player> winners)
        {
            // Simplified CTF logic - would need proper flag entities and team management
            winners = new List<Player>();
            return false;
        }
    }

    public class KingOfTheHillMode : GameMode
    {
        public override string Name => "King of the Hill";
        public override string Description => "Control the hill to earn points!";

        public KingOfTheHillMode(GameSettings settings) : base(settings) { }

        public override bool CheckWinCondition(out List<Player> winners)
        {
            // Simplified KOTH logic - would need hill zone and control tracking
            winners = new List<Player>();
            return false;
        }
    }
}
