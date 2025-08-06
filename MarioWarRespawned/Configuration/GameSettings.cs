using System.Collections.Generic;

namespace MarioWarRespawned.Configuration
{
    public class GameSettings
    {
        public GameModeType GameMode { get; set; } = GameModeType.Deathmatch;
        public int PlayerCount { get; set; } = 2;
        public List<string> PlayerNames { get; set; } = new() { "Mario", "Luigi", "Peach", "Toad" };
        public int KillLimit { get; set; } = 10;
        public float TimeLimit { get; set; } = 300; // 5 minutes in seconds
        public string MapName { get; set; } = "default";
        public bool FriendlyFire { get; set; } = true;
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Normal;

        public GameSettings()
        {
            // Ensure we have enough player names
            while (PlayerNames.Count < 4)
            {
                PlayerNames.Add($"Player {PlayerNames.Count + 1}");
            }
        }
    }

    public enum GameModeType
    {
        Deathmatch,
        CaptureTheFlag,
        KingOfTheHill,
        CoinCollection,
        Race
    }

    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard,
        Expert
    }
}
