using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GameLauncher.Models;
using GameLauncher.Services;

namespace GameLauncher.Views
{
    public partial class AddGamesDialog : Window
    {
        private readonly string _gamesPath;

        public AddGamesDialog()
        {
            InitializeComponent();
            // Use the app base directory so the editor / runtime will read/write the project's copy in the output folder.
            _gamesPath = "D:\\__Dev\\__C#\\GamesLauncher\\games.json";
        }

        private async void Ok_Click(object sender, RoutedEventArgs e)
        {
            var raw = GamesTextBox.Text ?? string.Empty;
            var titles = raw
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToArray();

            if (titles.Length == 0)
            {
                Close();
                return;
            }

            try
            {
                // Load existing games (returns empty list if file doesn't exist)
                var games = await JsonRepository.LoadAsync<Game>(_gamesPath);

                // Determine the starting order (highest existing + 1)
                var nextOrder = games.Any() ? games.Max(g => g.Order) + 1 : 0;

                foreach (var title in titles)
                {
                    var game = new Game
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = title,
                        Subtitle = "",
                        Order = nextOrder++,
                        Visible = true,
                        BgColor = "#000000",
                        LaunchEnvironments = new System.Collections.Generic.List<LaunchEnvironmentLink>
                        {
                            new LaunchEnvironmentLink { EnvId = "aero", LaunchType = "0", Path = "" },
                            new LaunchEnvironmentLink { EnvId = "ally", LaunchType = "0", Path = "" }
                        }
                    };

                    games.Add(game);
                }

                await JsonRepository.SaveAsync(_gamesPath, games);

                // Optionally inform the user and close
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add games: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}