using MarioWarRespawned.Configuration;

namespace MarioWarRespawned.Map
{
    /// <summary>
    /// Metadata associated with a map
    /// </summary>
    public class MapMetadata
    {
        public string Author { get; set; } = "Unknown";
        public string Description { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Normal;
        public string Theme { get; set; } = "Classic";
        public List<string> Tags { get; set; } = [];
        public TimeSpan BestTime { get; set; } = TimeSpan.Zero;
        public int TimesPlayed { get; set; } = 0;
        public float Rating { get; set; } = 0.0f;
    }
}