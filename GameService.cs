using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameLauncher.Models;
using System.Diagnostics;

namespace GameLauncher.Services
{
    public class GameService
    {
        // Fallback original dev paths (kept for backward compatibility)
        private const string FallbackGamesFilePath = @"G:\My Drive\#99_Otros documentos\GamesLauncher\games.json";
        private const string FallbackEnvironmentsFilePath = @"G:\My Drive\#99_Otros documentos\GamesLauncher\environments.json";
        private const string FallbackLaunchTypesFilePath = @"G:\My Drive\#99_Otros documentos\GamesLauncher\launchTypes.json";

        public List<Game> Games { get; private set; } = new();
        public List<LaunchEnvironment> Environments { get; private set; } = new();
        public List<LaunchType> LaunchTypes { get; private set; } = new();

        public async Task LoadAllAsync()
        {
            var gamesPath = FallbackGamesFilePath;
            var envPath = FallbackEnvironmentsFilePath;
            var typesPath = FallbackLaunchTypesFilePath;

            Debug.WriteLine($"[GameService] Loading games from: {gamesPath}");
            Debug.WriteLine($"[GameService] Loading environments from: {envPath}");
            Debug.WriteLine($"[GameService] Loading launch types from: {typesPath}");

            // Load games and ensure they are ordered by Order value
            Games = (await JsonRepository.LoadAsync<Game>(gamesPath))
                        .OrderBy(g => g.Order)
                        .ToList();

            Environments = await JsonRepository.LoadAsync<LaunchEnvironment>(envPath);
            LaunchTypes = await JsonRepository.LoadAsync<LaunchType>(typesPath);
        }

        public LaunchType GetLaunchTypeById(string id)
        {
            return LaunchTypes.FirstOrDefault(t => t.Id.ToString() == id);
        }

        public async Task SaveGamesAsync()
        {
            var gamesPath = FallbackGamesFilePath;
            // Ensure the service list is written as-is to disk.
            await JsonRepository.SaveAsync<Game>(gamesPath, Games);
        }

        private static string ResolvePath(string filename, string fallback)
        {
            var baseDir = AppContext.BaseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
            var candidates = new[]
            {
                Path.Combine(baseDir, filename),
                Path.Combine(baseDir, "Data", filename),
                Path.Combine(Directory.GetCurrentDirectory(), filename),
                fallback
            };

            var found = candidates.FirstOrDefault(File.Exists);
            return found ?? candidates[0]; // if none exists, return primary location (app base) so Save will write there
        }
    }
}
