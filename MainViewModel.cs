using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GameLauncher.Models;
using GameLauncher.Services;
using System;
using System.ComponentModel;
using System.Linq;

namespace GameLauncher.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Game> Games { get; set; } = new();
        public ObservableCollection<LaunchEnvironment> Environments { get; set; } = new();

        private readonly GameService _service = new();
        private const string ConfigFileName = "config.json";
        private string ConfigPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "GamesLauncher", ConfigFileName);

        private string _currentEnvironmentId;
        public string CurrentEnvironmentId
        {
            get => _currentEnvironmentId;
            set
            {
                if (_currentEnvironmentId != value)
                {
                    _currentEnvironmentId = value;
                    OnPropertyChanged(nameof(CurrentEnvironmentId));
                    SaveCurrentEnvironment();
                    RefreshGames(); // refresh UI when environment changes
                }
            }
        }

        private bool _showHidden = false;
        public bool ShowHidden
        {
            get => _showHidden;
            set
            {
                if (_showHidden != value)
                {
                    _showHidden = value;
                    OnPropertyChanged(nameof(ShowHidden));
                    RefreshGames();
                }
            }
        }

        public async Task InitializeAsync()
        {
            await _service.LoadAllAsync();

            Environments.Clear();
            foreach (var e in _service.Environments)
                Environments.Add(e);

            LoadCurrentEnvironment();

            // Refresh games after environments and current environment are available
            RefreshGames();
        }

        private void RefreshGames()
        {
            var filtered = _service.Games
                .Where(g => ShowHidden || g.Visible)
                .OrderBy(g => g.Order)
                .ToList();

            // Mark games that are disabled for the current environment (launch type "0")
            foreach (var g in filtered)
            {
                var envLink = g.LaunchEnvironments.FirstOrDefault(e => e.EnvId == CurrentEnvironmentId);
                g.IsDisabled = envLink != null && envLink.LaunchType == "0";
            }

            Games.Clear();
            foreach (var g in filtered)
                Games.Add(g);
        }

        // Moves a game from oldIndex to newIndex, updates Order values and persists to disk.
        public async Task MoveGameAsync(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= Games.Count) return;
            if (newIndex < 0) newIndex = 0;
            if (newIndex >= Games.Count) newIndex = Games.Count - 1;
            if (oldIndex == newIndex) return;

            var item = Games[oldIndex];
            Games.Move(oldIndex, newIndex);

            // Normalize orders to match the collection indices
            for (int i = 0; i < Games.Count; i++)
                Games[i].Order = i;

            // Update the service list and persist
            _service.Games.Clear();
            _service.Games.AddRange(Games);

            await _service.SaveGamesAsync();
        }

        private void LoadCurrentEnvironment()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var cfg = JsonSerializer.Deserialize<Config>(json);
                    if (cfg != null && !string.IsNullOrEmpty(cfg.CurrentEnvironmentId))
                        _currentEnvironmentId = cfg.CurrentEnvironmentId;
                    else
                        _currentEnvironmentId = Environments.FirstOrDefault()?.EnvId ?? "";
                }
                else
                {
                    _currentEnvironmentId = Environments.FirstOrDefault()?.EnvId ?? "";
                }

                // Raise notification after loading but avoid calling SaveCurrentEnvironment
                OnPropertyChanged(nameof(CurrentEnvironmentId));
            }
            catch
            {
                _currentEnvironmentId = Environments.FirstOrDefault()?.EnvId ?? "";
                OnPropertyChanged(nameof(CurrentEnvironmentId));
            }
        }

        private void SaveCurrentEnvironment()
        {
            try
            {
                var dir = Path.GetDirectoryName(ConfigPath)!;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var cfg = new Config { CurrentEnvironmentId = _currentEnvironmentId };
                File.WriteAllText(ConfigPath,
                    JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save environment: {ex.Message}");
            }
        }

        public void Launch(Game game)
        {
            if (string.IsNullOrEmpty(CurrentEnvironmentId)) return;

            var envLink = game.LaunchEnvironments.FirstOrDefault(e => e.EnvId == CurrentEnvironmentId);
            if (envLink == null) return;

            // If launch type is "0" treat as disabled/do not launch
            if (envLink.LaunchType == "0") return;

            var launchType = _service.GetLaunchTypeById(envLink.LaunchType);
            if (launchType == null) return;

            LauncherExecutor.Launch(game, envLink, launchType);
        }

        private class Config
        {
            public string CurrentEnvironmentId { get; set; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}