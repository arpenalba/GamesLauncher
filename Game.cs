using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GameLauncher.Models
{
    public class Game
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public int Order { get; set; }
        public bool Visible { get; set; }
        public string BgColor { get; set; }
        public List<LaunchEnvironmentLink> LaunchEnvironments { get; set; } = new();

        // UI-only flag (do not serialize to disk)
        [JsonIgnore]
        public bool IsDisabled { get; set; }
    }

    public class LaunchEnvironmentLink
    {
        public string EnvId { get; set; }
        public string LaunchType { get; set; }  // e.g. "1", "2"
        public string Path { get; set; }
    }
}
